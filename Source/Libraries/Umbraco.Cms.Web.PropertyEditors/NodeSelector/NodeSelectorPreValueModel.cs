using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataAnnotationsExtensions;
using Umbraco.Cms.Web.BuildManagerCodeDelegates;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.EmbeddedViewEngine;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Trees;
using Umbraco.Framework;
using Umbraco.Framework.Localization;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.PropertyEditors.NodeSelector
{
    [EmbeddedView("Umbraco.Cms.Web.PropertyEditors.NodeSelector.Views.NodeSelectorPreValueModel.cshtml", "Umbraco.Cms.Web.PropertyEditors")]
    [Bind(Exclude = "AvailableTrees")]
    public class NodeSelectorPreValueModel : PreValueModel, IValidatableObject
    {
        private readonly List<Lazy<TreeController, TreeMetadata>> _trees;

        public NodeSelectorPreValueModel(IEnumerable<Lazy<TreeController, TreeMetadata>> trees)
        {
            _trees = trees.ToList();
            EditorHeight = 300;
            ShowToolTip = true;
            MinNodeCount = 0;
            MaxNodeCount = 0;
            NodeFilter = "return true;";
            FilterType = NodeFilterType.Enabled;
            StartNodeSelectionType = StartNodeSelectionType.UsePicker;
            StartNodeQuery = "return FixedHiveIds.ContentVirtualRoot;";
            ThumbnailPropertyName = "image";
        }

        [Display(Description = "NodeSelectorPreValueModel.StartNodeQuery.Description")]
        public string StartNodeQuery { get; set; }

        [Display(Description = "NodeSelectorPreValueModel.StartNodeSelectionType.Description")]
        public StartNodeSelectionType StartNodeSelectionType { get; set; }

        [Display(Description = "NodeSelectorPreValueModel.FilterType.Description")]
        public NodeFilterType FilterType { get; set; }

        [Display(Description = "NodeSelectorPreValueModel.NodeFilter.Description")]
        public string NodeFilter { get; set; }

        /// <summary>
        /// Gets or sets the available trees.
        /// </summary>
        /// <value>
        /// The available trees.
        /// </value>
        [ReadOnly(true)]
        [ScaffoldColumn(false)]
        public IEnumerable<SelectListItem> AvailableTrees { get; set; }

        /// <summary>
        /// The Id of the selected tree to render
        /// </summary>        
        [Display(Description = "TreePicker.SelectedTree.Description")]
        [Required]
        public Guid SelectedTree { get; set; }

        /// <summary>
        /// The start node id used when the node selection is a picker
        /// </summary>
        public HiveId StartNodeId { get; set; }

        /// <summary>
        /// A boolean value indicating whether or not to show the thumbnails
        /// </summary>        
        [Display(Description = "NodeSelectorPreValueModel.ShowThumbnails.Description")]
        public bool ShowThumbnails { get; set; }

        /// <summary>
        /// The property name used as the source of the thumbnail to show
        /// </summary>
        [Display(Description = "NodeSelectorPreValueModel.ThumbnailPropertyName.Description")]
        public string ThumbnailPropertyName { get; set; }

        ///<summary>
        /// The editor height in pixels
        ///</summary>        
        [Range(290, 1000)]
        [Display(Description = "NodeSelectorPreValueModel.EditorHeight.Description")]
        public int EditorHeight { get; set; }

        /// <summary>
        /// Whether or not to display the tooltip
        /// </summary>
        public bool ShowToolTip { get; set; }
        
        /// <summary>
        /// The minimum amount of nodes that are able to be selected
        /// </summary>
        [AllowDocumentTypePropertyOverride]
        [Display(Description = "NodeSelectorPreValueModel.MinNodeCount.Description")]
        [Range(0, int.MaxValue)]
        public int MinNodeCount { get; set; }
		
        /// <summary>
        /// The maximum amount of nodes that are able to be selected
        /// </summary>
        [AllowDocumentTypePropertyOverride]
        [Display(Description = "NodeSelectorPreValueModel.MaxNodeCount.Description")]
        [Range(0, int.MaxValue)]        
        public int MaxNodeCount { get; set; }

        /// <summary>
        /// Returns the trees available within umbraco
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// The value portion is the JsonObject of the HiveId which is wrapping the tree GUID Id since we need to have the JSON version of the
        /// HiveId in JavaScript and we don't have a JS parser/generator for HiveId since we'd like to keep this in c# only.
        /// </remarks>
        protected virtual IEnumerable<SelectListItem> GetAvailableTrees()
        {
            return _trees
                .Select(x => new SelectListItem
                {
                    Text = x.Metadata.TreeTitle,
                    Value = x.Metadata.Id.ToString(),
                    Selected = SelectedTree == x.Metadata.Id
                })
                .OrderBy(x => x.Text)
                .ToList();
        }

        /// <summary>
        /// Assign the AvailableTrees when model values are set and ensure the correct tree is selected in the 
        /// select list.
        /// </summary>
        /// <param name="serializedVal"></param>
        public override void SetModelValues(string serializedVal)
        {            
            base.SetModelValues(serializedVal);

            AvailableTrees = GetAvailableTrees();

            //need to validate whether the selected tree actually exists anymore, this will occur if a tree is removed from code.
            if (_trees.SingleOrDefault(x => x.Metadata.Id == SelectedTree) == null)
            {
                SelectedTree = Guid.Empty;
            }
        }

		/// <summary>
        /// Executes custom server side validation for the model
        /// </summary>
        /// <param name="validationContext"></param>
        /// <returns></returns>
		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (MinNodeCount > MaxNodeCount)
            {
                //return a validation result with a message and for which field the error is associated
                yield return new ValidationResult("NodeSelectorPreValueModel.NodeCount.Error".Localize(), new[] { "Value" });
            }
        }

    }
}