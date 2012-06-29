using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Umbraco.Cms.Web.BuildManagerCodeDelegates;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Cms.Web.Trees;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;

[assembly: WebResource("Umbraco.Cms.Web.PropertyEditors.NodeSelector.Resources.NodeSelectorEditor.js", "application/x-javascript")]
[assembly: WebResource("Umbraco.Cms.Web.PropertyEditors.NodeSelector.Resources.NodeSelectorPreValues.js", "application/x-javascript")]
[assembly: WebResource("Umbraco.Cms.Web.PropertyEditors.NodeSelector.Resources.NodeSelectorEditor.css", "text/css", PerformSubstitution = true)]
[assembly: WebResource("Umbraco.Cms.Web.PropertyEditors.NodeSelector.Resources.NodeSelectorPreValues.css", "text/css", PerformSubstitution = true)]
[assembly: WebResource("Umbraco.Cms.Web.PropertyEditors.NodeSelector.Resources.delete-icon.gif", "image/gif")]
[assembly: WebResource("Umbraco.Cms.Web.PropertyEditors.NodeSelector.Resources.throbber.gif", "image/gif")]
[assembly: WebResource("Umbraco.Cms.Web.PropertyEditors.NodeSelector.Resources.info-icon.png", "image/png")]

namespace Umbraco.Cms.Web.PropertyEditors.NodeSelector
{

    [PropertyEditor(CorePluginConstants.NodeSelectorEditorId, "NodeSelector", "Node Selector", IsParameterEditor = false)]
    public class NodeSelectorEditor : ContentAwarePropertyEditor<NodeSelectorEditorModel, NodeSelectorPreValueModel>
    {
	
		protected IBackOfficeRequestContext BackOfficeRequestContext {get; private set;}

		/// <summary>
        /// Initializes a new instance of the <see cref="NodeSelectorEditor"/> class.
        /// </summary>
        /// <param name="backOfficeRequestContext">The back office request context.</param>
        public NodeSelectorEditor(IBackOfficeRequestContext backOfficeRequestContext)
        {
            BackOfficeRequestContext = backOfficeRequestContext;
        }

		
		/// <summary>
        /// Creates the editor model
        /// </summary>
        /// <param name="preValues"></param>
        /// <returns></returns>
		public override NodeSelectorEditorModel CreateEditorModel(NodeSelectorPreValueModel preValues)
        {
            var docType = GetContentPropertyValue(x => x.DocTypeProperty, null);
            var dataTypeId = HiveId.Empty;
		    var currentNodeId = GetContentModelValue(x => x.Id, HiveId.Empty);
            if (docType != null)
            {
                dataTypeId = docType.DataTypeId;
            }

            var model = new NodeSelectorEditorModel(
                preValues, 
                BackOfficeRequestContext, 
                GetContentModelValue(x => x.Id, HiveId.Empty),
                GetContentPropertyValue(x => x.Alias, ""),
                dataTypeId);

            //not sure what else to do here, but need a UrlHelper which requires a RequestContext
		    var urlHelper = DependencyResolver.Current.GetService<UrlHelper>();

            //give the HtmlId of the tree being rendered a unique but consistent Id across the editor
		    var htmlId = "ns_" + GetContentPropertyValue(x => x.Id, HiveId.Empty).GetHtmlId();

            //create the custom tree parameters that will be passed up in query string
		    var treeparams = urlHelper.CreateTreeParams(
		        true, //it is dialog
		        "Umbraco.PropertyEditors.NodeSelector.nodeClickHandler", //the node click handler
		        new Dictionary<string, object>
		            {
                        //we want to render the node requested, not its children
		                {TreeQueryStringParameters.RenderParent, true}
                    });

		    var startNodeId = HiveId.Empty;

            if (preValues.SelectedTree == default(Guid))
            {
                model.ErrorMessage = string.Format("No tree type has been selected for the NodeSelector");
                //exit now as we cannot continue
                return model;
            }

            //we need to get the start node id based on the query
            var ds = BackOfficeRequestContext.RegisteredComponents.TreeControllers.GetNodeSelectorDataSource(preValues.SelectedTree);

            if (preValues.StartNodeSelectionType == StartNodeSelectionType.UsePicker)
            {
                if (!preValues.StartNodeId.IsNullValueOrEmpty())
                {
                    model.StartNode = ds.GetEntity(preValues.StartNodeId);
                    if (model.StartNode == null)
                    {
                        model.ErrorMessage = string.Format("Could not find a start node with Id: {0} to render", preValues.StartNodeId.ToFriendlyString());
                        //exit now as we cannot continue
                        return model;
                    }
                }
                else
                {
                    model.ErrorMessage = string.Format("No start node id assigned to the NodeSelector");
                    //exit now as we cannot continue
                    return model;
                }

                startNodeId = preValues.StartNodeId;
            }
            else
            {
                if (dataTypeId != HiveId.Empty && currentNodeId != HiveId.Empty)
                {
                    //TODO: Error check this nicely !!
                    startNodeId = CSharpExpressionsUtility.GetStartNodeQueryResult(BackOfficeRequestContext, dataTypeId, ds, currentNodeId);
                }
                
            }

		    model.TreeModel = new TreeRenderModel(
		        urlHelper.GetTreeUrl(startNodeId, preValues.SelectedTree, treeparams))
		        {                    
                    TreeHtmlElementId = htmlId,
                    ShowContextMenu = false
		        };

		    return model;
        }
		
		/// <summary>
        /// Creates the pre value model
        /// </summary>
        /// <returns></returns>
        public override NodeSelectorPreValueModel CreatePreValueEditorModel()
		{
            //only allow trees that are NodeSelectorCompatible
		    var filteredControllers = BackOfficeRequestContext.RegisteredComponents.TreeControllers
		        .Where(x => x.Metadata.ComponentType.GetCustomAttributes<NodeSelectorCompatibleAttribute>(true).Any());

		    return new NodeSelectorPreValueModel(filteredControllers);
		}

        
		
	}
}