using Rebel.Cms.Web.Model.BackOffice.Trees;

namespace Rebel.Cms.Web.PropertyEditors.NodeSelector
{
    public static class TreeNodeExtensions
    {
        public static void SetSelectable(this TreeNode node, bool isSelectable)
        { 
            node.AdditionalData[TreeNodeMetaData.IsSelectable] = false;         
        }
    }
}