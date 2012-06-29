using System;
using System.Security.Policy;
using System.Text;
using System.Web;
using System.Web.Compilation;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Security;
using Umbraco.Cms.Web.Security.Permissions;
using Umbraco.Cms.Web.Trees;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.DependencyManagement;
using Umbraco.Framework.Dynamics.Expressions;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Tasks;
using Umbraco.Framework.Security;
using System.Linq;

namespace Umbraco.Cms.Web.PropertyEditors.NodeSelector
{
    /// <summary>
    /// A NodeSelector Content compatible tree
    /// </summary>
    [NodeSelectorCompatible(typeof(NodeSelectorContentTreeController))]
    [Tree(FixedTreeIds.NodeSelectorContentTreeId, "Content")]
    public class NodeSelectorContentTreeController : ContentTreeController, INodeSelectorDataSource
    {
        public NodeSelectorContentTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
            _helper = new NodeSelectorContentTreeControllerHelper(GetHiveProvider, EnsureInitialized, GetTreeNodeAlias, BackOfficeRequestContext);
        }

        private readonly NodeSelectorContentTreeControllerHelper _helper;

        /// <summary>
        /// We override the RootNodeId because if a request is received with a querystring directive of
        /// TreeQueryStringParameters.RenderParent, then we'll dynamically set the RootNodeId to the one
        /// being requested.
        /// </summary>
        protected override HiveId RootNodeId
        {
            get { return _helper.GetRootNodeIdForRequest(() => base.RootNodeId); }
        }

        /// <summary>
        /// Overrides creating the root node so we can ensure the correct metadata is applied to it.
        /// </summary>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected override TreeNode CreateRootNode(FormCollection queryStrings)
        {
            return _helper.CreateRootNode(queryStrings, base.CreateRootNode);
        }

        /// <summary>
        /// Returns the tree data for the specified node taking into account if a TreeQueryStringParameters.RenderParent
        /// is set to true.
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected override UmbracoTreeResult GetTreeData(HiveId parentId, FormCollection queryStrings)
        {
            return _helper.GetTreeData(parentId, queryStrings, ActualRootNodeId, NodeCollection, CreateRootNode, UmbracoTree, base.GetTreeData);
        }

        /// <summary>
        /// Returns the model for each NodeSelector persisted item
        /// </summary>
        /// <param name="id"></param>
        /// <param name="treeId"> </param>
        /// <returns></returns>
        public SelectedItemModel GetNodeSelectorItemModel(HiveId id, Guid treeId)
        {
            return _helper.GetNodeSelectorItemModel(id, treeId, () => Url);
        }

        /// <summary>
        /// Returns the paths for the specified HiveId
        /// </summary>
        /// <param name="id"></param>
        /// <param name="treeId"> </param>
        /// <returns></returns>
        public EntityPathCollection GetPaths(HiveId id, Guid treeId)
        {
            return _helper.GetPaths(id, treeId);
        }

        /// <summary>
        /// Returns the content to display in the tooltip
        /// </summary>
        /// <param name="id"></param>
        /// <param name="treeId"></param>
        /// <returns></returns>
        public TooltipContents GetTooltipContents(HiveId id, Guid treeId)
        {
            return _helper.GetTooltipContents(
                id, treeId,
                (obj, te, html) => this.CreateTooltipContentsViaTask(obj, te, html),
                GetEditorUrl);
        }

        /// <summary>
        /// Returns the url for the media item
        /// </summary>
        /// <param name="id"></param>
        /// <param name="treeId"></param>
        /// <param name="propertyAlias"></param>
        /// <returns></returns>
        public string GetMediaUrl(HiveId id, Guid treeId, string propertyAlias)
        {
            return _helper.GetMediaUrl(id, treeId, propertyAlias, () => Url);
        }

        /// <summary>
        /// Returns an entity given a HiveId
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public TypedEntity GetEntity(HiveId nodeId)
        {
            return _helper.GetEntity(nodeId);
        }

        /// <summary>
        /// Returns the root node id defined by the tree
        /// </summary>
        /// <returns></returns>
        public HiveId GetRootNodeId()
        {
            return _helper.GetRootNodeIdForTree(() => RootNodeId);
        }

        /// <summary>
        /// Used by the INodeSelectorDataSource because it executes in a context outside of a normal request,
        /// so we need to ensure this controller is initialized so we resolve the current request from IoC and 
        /// manually initialize.
        /// </summary>
        private void EnsureInitialized()
        {
            if (Request == null)
            {
                var request = DependencyResolver.Current.GetService<HttpRequestBase>();
                this.Initialize(request.RequestContext);
            }
        }

        public IFrameworkContext FrameworkContext
        {
            get { return BackOfficeRequestContext.Application.FrameworkContext; }
        }
    }
}
