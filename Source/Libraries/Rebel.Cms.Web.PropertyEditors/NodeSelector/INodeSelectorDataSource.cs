using System;
using System.Web.Mvc;
using Rebel.Framework;
using Rebel.Framework.Context;
using Rebel.Framework.Persistence.Model;

namespace Rebel.Cms.Web.PropertyEditors.NodeSelector
{
    /// <summary>
    /// Any NodeSelectorCompatible tree must declare an instance of this type in order for the NodeSelector to resolve
    /// the model for each persisted item.
    /// </summary>
    public interface INodeSelectorDataSource : IRequiresFrameworkContext
    {
        /// <summary>
        /// Returns the model representing each persisted item
        /// </summary>
        /// <param name="id"></param>
        /// /// <param name="treeId">The Id of the tree type being rendered</param>
        /// <returns></returns>
        SelectedItemModel GetNodeSelectorItemModel(HiveId id, Guid treeId);
        
        /// <summary>
        /// Returns the path data used to sync the tree
        /// </summary>
        /// <param name="id"></param>
        /// /// <param name="treeId">The Id of the tree type being rendered</param>
        /// <returns></returns>
        EntityPathCollection GetPaths(HiveId id, Guid treeId);

        /// <summary>
        /// Returns the contents to display in the tooltip for the given element Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="treeId">The Id of the tree type being rendered</param>
        /// <returns></returns>
        TooltipContents GetTooltipContents(HiveId id, Guid treeId);

        /// <summary>
        /// Returns the media URL for the specified media item based on the attribute alias
        /// </summary>
        /// <param name="id"></param>
        /// <param name="treeId"></param>
        /// <param name="propertyAlias"></param>
        /// <returns></returns>
        string GetMediaUrl(HiveId id, Guid treeId, string propertyAlias);

        /// <summary>
        /// Returns a TypedEntity based on the id
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        TypedEntity GetEntity(HiveId nodeId);

        /// <summary>
        /// Returns the root node id for the current tree
        /// </summary>
        /// <returns></returns>
        HiveId GetRootNodeId();

    }
}