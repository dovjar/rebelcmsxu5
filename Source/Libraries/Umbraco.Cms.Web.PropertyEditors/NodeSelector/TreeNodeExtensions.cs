using Umbraco.Cms.Web.Model.BackOffice.Trees;

namespace Umbraco.Cms.Web.PropertyEditors.NodeSelector
{
    public static class TreeNodeExtensions
    {
        public static void SetSelectable(this TreeNode node, bool isSelectable)
        { 
            node.AdditionalData[TreeNodeMetaData.IsSelectable] = false;         
        }
    }
}