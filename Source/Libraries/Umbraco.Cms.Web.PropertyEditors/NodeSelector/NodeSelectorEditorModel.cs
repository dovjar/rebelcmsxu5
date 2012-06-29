using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.EmbeddedViewEngine;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Framework;
using Umbraco.Framework.Localization;
using Umbraco.Framework.Persistence.Model;

namespace Umbraco.Cms.Web.PropertyEditors.NodeSelector
{
    [EmbeddedView("Umbraco.Cms.Web.PropertyEditors.NodeSelector.Views.NodeSelectorEditorModel.cshtml", "Umbraco.Cms.Web.PropertyEditors")]
    [Bind(Exclude = "TreeModel,StartNode,ErrorMessage,PersistedItems,DataTypeId")]
    public class NodeSelectorEditorModel : EditorModel<NodeSelectorPreValueModel>, IValidatableObject
    {
        /// <summary>
        /// Used in a hidden field in order for a JSON request that is required to be sent to the server which contains the current
        /// property's data type id
        /// </summary>
        /// <remarks>
        /// We only want the string representation of the HiveId
        /// </remarks>
        [ReadOnly(true)]
        [HiddenInput(DisplayValue = false)]
        public HiveId DataTypeId { get; set; }

        [ReadOnly(true)]
        [ScaffoldColumn(false)]
        public IEnumerable<SelectedItemModel> PersistedItems { get; set; }

        /// <summary>
        /// The tree model used to render the tree
        /// </summary>
        [ReadOnly(true)]
        [ScaffoldColumn(false)]
        public TreeRenderModel TreeModel { get; set; }

        /// <summary>
        /// The start node to display above the tree
        /// </summary>
        [ReadOnly(true)]
        [ScaffoldColumn(false)]
        public TypedEntity StartNode { get; set; }

        /// <summary>
        /// Displays an error message that may occur when trying to create the model
        /// </summary>
        [ReadOnly(true)]
        [ScaffoldColumn(false)]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets/sets the value
        /// </summary>
        [HiddenInput(DisplayValue = false)]
        public string Value { get; set; }

        #region Private members
        private readonly IBackOfficeRequestContext _backOfficeRequestContext;
        private readonly HiveId _contentId;
        private readonly string _propertyAlias;
        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeSelectorEditorModel"/> class.
        /// </summary>
        /// <param name="preValueModel"> </param>
        /// <param name="backOfficeRequestContext">The back office request context.</param>
        /// <param name="contentId">The current id of the content item being rendered.</param>
        /// <param name="propertyAlias">The current alias of the property being rendered.</param>
        /// <param name="dataTypeId"> </param>
        public NodeSelectorEditorModel(
            NodeSelectorPreValueModel preValueModel,
            IBackOfficeRequestContext backOfficeRequestContext,
            HiveId contentId,
            string propertyAlias,
            HiveId dataTypeId)
            : base(preValueModel)
        {
            _backOfficeRequestContext = backOfficeRequestContext;
            _contentId = contentId;
            _propertyAlias = propertyAlias;
            DataTypeId = dataTypeId;
        }
        #endregion

        /// <summary>
        /// Returns a serialized value for the Editor Model
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This Editor stores a list of saved items, each list item key name is simply 'val' + the index of the item.
        /// When using this value in the front end, the key shouldn't really matter in usage.
        /// </remarks>
        public override IDictionary<string, object> GetSerializedValue()
        {
            var vals = new Dictionary<string, object>();
            if (Value.IsNullOrWhiteSpace())
                return vals;

            var count = 0;
            foreach (var item in Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                vals.Add("val" + count, item);
                count++;
            }

            return vals;
        }

        /// <summary>
        /// Builds up the PersistedItems property
        /// </summary>
        /// <param name="serializedVal"></param>
        public override void SetModelValues(IDictionary<string, object> serializedVal)
        {
            //set the Value to a serialized version of what is saved

            var persistedIds = serializedVal
                .Where(x => x.Key.StartsWith("val"))
                //need to ensure they are sorted!
                .OrderBy(x =>
                    {
                        var index = x.Key.Substring(3, x.Key.Length - 3);
                        int i;
                        return int.TryParse(index, out i) ? i : 0;
                    })
                .Select(x => x.Value.ToString())
                .WhereNotNull()
                .ToArray();

            Value = string.Join(",", persistedIds);

            //now we have to build up the PersistedItems property, we need to determine the node's icon, name, id , etc...
            //we do this by getting the corresponding INodeSelectorDataSource for the tree that has been selected in prevalues.

            var dataSource = _backOfficeRequestContext.RegisteredComponents.TreeControllers
                .GetNodeSelectorDataSource(PreValueModel.SelectedTree);


            var hiveIds = persistedIds
                .Select(x =>
                    {
                        //need to do some error checking here to ensure it parses correctly
                        var parsedId = HiveId.TryParse(x);
                        return parsedId.Success ? parsedId.Result : HiveId.Empty;
                    })
                .Where(x => x != HiveId.Empty)
                .ToArray();

            //set the persisted items
            PersistedItems = hiveIds
                .Select(x =>
                    {
                        var model = dataSource.GetNodeSelectorItemModel(x, PreValueModel.SelectedTree);
                        //if showing thumbs, go get it
                        if (PreValueModel.ShowThumbnails)
                        {
                            model.ThumbnailUrl = dataSource.GetMediaUrl(x, PreValueModel.SelectedTree, PreValueModel.ThumbnailPropertyName);
                        }
                        return model;
                    })
                .Where(x => x != null)
                .ToList();
            
        }


        /// <summary>
        /// Executes custom server side validation for the model
        /// </summary>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {

            var totalSelected =
                Value.IsNullOrWhiteSpace()
                    ? 0
                    : Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Length;
            if (PreValueModel.MaxNodeCount > 0 && totalSelected > PreValueModel.MaxNodeCount)
            {
                yield return new ValidationResult("NodeSelectorEditorModel.MaxNodeCount.Error".Localize(this, parameters: new { Max = PreValueModel.MaxNodeCount }), new[] { "Value" });
            }
            if (PreValueModel.MinNodeCount > 0 && totalSelected < PreValueModel.MinNodeCount)
            {
                yield return new ValidationResult("NodeSelectorEditorModel.MinNodeCount.Error".Localize(this, parameters: new { Min = PreValueModel.MinNodeCount }), new[] { "Value" });
            }
        }

    }
}