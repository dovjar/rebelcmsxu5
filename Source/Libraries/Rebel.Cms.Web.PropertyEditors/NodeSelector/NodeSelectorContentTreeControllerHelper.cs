using System;
using System.Web.Mvc;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Cms.Web.Trees;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Hive;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Cms.Web.PropertyEditors.NodeSelector
{
    /// <summary>
    /// A helper class that is used by both the Media and Content enabled NodeSelector trees in order to 
    /// not duplicate a ton of code.
    /// </summary>
    internal class NodeSelectorContentTreeControllerHelper
    {
        private readonly Func<HiveId, FormCollection, ReadonlyGroupUnitFactory> _getHiveProvider;
        private readonly Action _ensureInitialized;
        private readonly Func<TypedEntity, string> _getTreeNodeAlias;
        private readonly IBackOfficeRequestContext _backOfficeRequestContext;

        public NodeSelectorContentTreeControllerHelper(
            Func<HiveId, FormCollection, ReadonlyGroupUnitFactory> getHiveProvider,
            Action ensureInitialized,
            Func<TypedEntity, string> getTreeNodeAlias,
            IBackOfficeRequestContext backOfficeRequestContext)
        {
            _getHiveProvider = getHiveProvider;
            _ensureInitialized = ensureInitialized;
            _getTreeNodeAlias = getTreeNodeAlias;
            _backOfficeRequestContext = backOfficeRequestContext;
        }

        private HiveId _dynamicRootNodeId = HiveId.Empty;

        /// <summary>
        /// If a request is received with a querystring directive of
        /// TreeQueryStringParameters.RenderParent, then we'll dynamically set the RootNodeId to the one
        /// being requested.
        /// </summary>
        internal HiveId GetRootNodeIdForRequest(Func<HiveId> getRootNodeId)
        {
            return _dynamicRootNodeId == HiveId.Empty
                       ? getRootNodeId()
                       : _dynamicRootNodeId;            
        }

        /// <summary>
        /// Check if the querystring directive of TreeQueryStringParameters.RenderParent is true, if so then
        /// set the _dynamicRootNodeId value to the one being requested and only return the root node, otherwise
        /// proceed as normal.
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="queryStrings"></param>
        /// <param name="actualRootNodeId"> </param>
        /// <param name="nodeCollection"> </param>
        /// <param name="createRootNode"> </param>
        /// <param name="returnResult"> </param>
        /// <param name="returnDefaultResult"> </param>
        /// <returns></returns>
        internal RebelTreeResult GetTreeData(
            HiveId parentId, 
            FormCollection queryStrings, 
            HiveId actualRootNodeId,
            TreeNodeCollection nodeCollection,
            Func<FormCollection, TreeNode> createRootNode,
            Func<RebelTreeResult> returnResult,
            Func<HiveId, FormCollection, RebelTreeResult> returnDefaultResult)
        {
            if (queryStrings.GetValue<bool>(TreeQueryStringParameters.RenderParent))
            {
                //if the requested node does not equal the root node set, then set the _dynamicRootNodeId
                if (!parentId.Value.Equals(actualRootNodeId.Value))
                {
                    _dynamicRootNodeId = parentId;
                }
                //need to remove the query strings we don't want passed down to the children.
                queryStrings.Remove(TreeQueryStringParameters.RenderParent);
                nodeCollection.Add(createRootNode(queryStrings));
                return returnResult();
            }

            return returnDefaultResult(parentId, queryStrings);
        }

        /// <summary>
        /// Creates the root node so we can ensure the correct metadata is applied to it.
        /// </summary>
        /// <param name="queryStrings"></param>
        /// <param name="createRootNode"> </param>
        /// <returns></returns>
        internal TreeNode CreateRootNode(FormCollection queryStrings, Func<FormCollection, TreeNode> createRootNode)
        {
            var rootNode = createRootNode(queryStrings);
            rootNode.SetSelectable(false);
            return rootNode;
        }

        /// <summary>
        /// Returns the model for each NodeSelector persisted item
        /// </summary>
        /// <param name="id"></param>
        /// <param name="treeId"> </param>
        /// <param name="url"> </param>
        /// <returns></returns>
        internal SelectedItemModel GetNodeSelectorItemModel(HiveId id, Guid treeId, Func<UrlHelper> url)
        {
            _ensureInitialized();

            var model = new SelectedItemModel();

            var entity = GetEntity(id);

            if (entity == null)
                return null;
            model.NodeName = _getTreeNodeAlias(entity);
            model.NodeId = entity.Id;
            var icon = entity.EntitySchema.GetXmlConfigProperty("icon");
            if (!string.IsNullOrEmpty(icon))
            {
                if (icon.Contains("."))
                {
                    model.NodeIcon =
                        url().Content(
                            _backOfficeRequestContext.Application.Settings.RebelFolders.
                                DocTypeIconFolder + "/" + icon);
                }
                else
                {
                    model.NodeStyle = icon;
                }
            }            
            
            return model;
        }

        /// <summary>
        /// Returns the paths for the specified HiveId
        /// </summary>
        /// <param name="id"></param>
        /// <param name="treeId"> </param>
        /// <returns></returns>
        internal EntityPathCollection GetPaths(HiveId id, Guid treeId)
        {
            var hiveProvider = _getHiveProvider(id, new FormCollection());
            using (var uow = hiveProvider.CreateReadonly<IContentStore>())
            {
                var entity = uow.Repositories.GetById(id);
                return uow.Repositories.GetEntityPaths(entity.Id, FixedRelationTypes.DefaultRelationType);
            }
        }

        /// <summary>
        /// Returns the content to display in the tooltip
        /// </summary>
        /// <param name="id"></param>
        /// <param name="treeId"></param>
        /// <param name="createTooltipContents"> </param>
        /// <param name="getEditorUrl"> </param>
        /// <returns></returns>
        internal TooltipContents GetTooltipContents(HiveId id, Guid treeId,
                                                    Func<object, TypedEntity, string, TooltipContents> createTooltipContents,
                                                    Func<HiveId, FormCollection, string> getEditorUrl)
        {
            _ensureInitialized();

            const string template = "<a href='{0}'>[edit]</a><ul><li class='id'>ID value:<br/><strong>{1}</strong></li><li class='soft'>Provider group: <strong>{2}</strong></li><li class='soft'>Provider id: <strong>{3}</strong></li></ul><p><i>{4}</i></p>";
            var hiveProvider = _getHiveProvider(id, new FormCollection());
            using (var uow = hiveProvider.CreateReadonly<IContentStore>())
            {
                var entity = uow.Repositories.GetById(id);

                //return the tooltip contents and proxy through task system
                return createTooltipContents(this, entity,
                                             string.Format(template,
                                                           getEditorUrl(id, new FormCollection()),
                                                           entity.Id.Value,
                                                           entity.Id.ProviderGroupRoot,
                                                           entity.Id.ProviderId,
                                                           _getTreeNodeAlias(entity)));
            }
        }

        /// <summary>
        /// Returns the url for the media item
        /// </summary>
        /// <param name="id"></param>
        /// <param name="treeId"></param>
        /// <param name="propertyAlias"></param>
        /// <param name="url"> </param>
        /// <returns></returns>
        internal string GetMediaUrl(HiveId id, Guid treeId, string propertyAlias, Func<UrlHelper> url)
        {
            _ensureInitialized();
            return url().GetMediaUrl(id, propertyAlias, 100);
        }

        /// <summary>
        /// Returns an entity given a HiveId
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        internal TypedEntity GetEntity(HiveId nodeId)
        {
            var hiveProvider = _getHiveProvider(nodeId, new FormCollection());
            using (var uow = hiveProvider.CreateReadonly<IContentStore>())
            {
                return uow.Repositories.Get(nodeId);
            }
        }

        /// <summary>
        /// Returns the root node id defined by the tree
        /// </summary>
        /// <returns></returns>
        internal HiveId GetRootNodeIdForTree(Func<HiveId> rootNodeId)
        {
            _ensureInitialized();
            return rootNodeId();

        }
    }
}