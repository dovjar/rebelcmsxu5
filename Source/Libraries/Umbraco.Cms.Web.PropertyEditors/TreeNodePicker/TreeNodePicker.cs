using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Trees;

namespace Umbraco.Cms.Web.PropertyEditors.TreeNodePicker
{
    [PropertyEditor(CorePluginConstants.TreeNodePickerPropertyEditorId, "TreeNodePicker", "Tree Node Picker", IsParameterEditor = true)]
    public class TreeNodePicker : PropertyEditor<TreeNodePickerModel, TreeNodePickerPreValueModel>
    {
        private readonly IEnumerable<Lazy<TreeController, TreeMetadata>> _trees;

        public TreeNodePicker(IEnumerable<Lazy<TreeController, TreeMetadata>> trees)
        {
            _trees = trees;
        }

        public override TreeNodePickerModel CreateEditorModel(TreeNodePickerPreValueModel preValues)
        {
            return new TreeNodePickerModel(preValues, _trees);
        }

        public override TreeNodePickerPreValueModel CreatePreValueEditorModel()
        {
            //only allow trees registered with the TreePickers collection
            return new TreeNodePickerPreValueModel(_trees.Where(x => TreePickers.Trees.Contains(x.Metadata.Id)));
        }
    }
}
