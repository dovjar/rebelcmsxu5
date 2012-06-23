using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Model.BackOffice.Trees;
using RebelCms.Cms.Web.Mvc.ActionFilters;
using RebelCms.Cms.Web.Trees.MenuItems;
using RebelCms.Framework;
using RebelCms.Framework.Persistence;
using RebelCms.Framework.Persistence.Model;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Persistence.Model.Constants.AttributeDefinitions;
using RebelCms.Framework.Security;
using RebelCms.Hive.ProviderGrouping;
using RebelCms.Hive.RepositoryTypes;
using RebelCms.Hive;

namespace RebelCms.Cms.Web.Trees
{
    /// <summary>
    /// A tree that supports a recycle bin
    /// </summary>
    public abstract class RecycleBinTreeController : SupportsEditorTreeController
    {
        protected RecycleBinTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
        }

        protected abstract ReadonlyGroupUnitFactory GetHiveProvider(HiveId parentId, FormCollection queryStrings);

        public abstract HiveId RecycleBinId { get; }

        protected override RebelCmsTreeResult GetTreeData(HiveId parentId, FormCollection queryStrings)
        {
            foreach (var treeNode in GetChildTreeNodes(parentId, queryStrings))
            {
                if (parentId == RecycleBinId)
                {
                    AddMenuItemsToNodeInRecycleBin(treeNode, queryStrings);
                }
                else
                {
                    AddMenuItemsToNode(treeNode, queryStrings);    
                }
                NodeCollection.Add(treeNode);
            }

            //add the recycle bin if required
            this.TryExecuteSecuredMethod(x => x.AddRecycleBin(parentId, queryStrings));

            return RebelCmsTree();

        }

        /// <summary>
        /// Adds menu items to each node that is not under the recycle bin
        /// </summary>
        /// <param name="n"></param>
        /// <param name="queryStrings"></param>
        protected abstract void AddMenuItemsToNode(TreeNode n, FormCollection queryStrings);

        /// <summary>
        /// Adds menu itmes to each node that is in the recycle bin
        /// </summary>
        /// <param name="n"></param>
        /// <param name="queryStrings"></param>
        protected abstract void AddMenuItemsToNodeInRecycleBin(TreeNode n, FormCollection queryStrings);

        /// <summary>
        /// Gets the child tree nodes for any node other than the recycle bin children
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected abstract IEnumerable<TreeNode> GetChildTreeNodes(HiveId parentId, FormCollection queryStrings);

        /// <summary>
        /// Adds the recycle bin if it is the first node in the tree
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="queryStrings"></param>
        [RebelCmsAuthorize(Permissions = new[] { FixedPermissionIds.View }, IdParameterName = "parentId")]
        protected virtual void AddRecycleBin(HiveId parentId, FormCollection queryStrings)
        {
            //check if this node is the first node in the tree, if so, then append the recycle bin.
            //if the tree is in dialog mode then ignore
            if (RecycleBinId.IsNullValueOrEmpty() || (parentId != RootNodeId) || queryStrings.GetValue<bool>(TreeQueryStringParameters.DialogMode)) 
                return;

            var hiveProvider = GetHiveProvider(parentId, queryStrings);
            using (var uow = hiveProvider.CreateReadonly<IContentStore>())
            {
                var recycleBinEntity = uow.Repositories.Get(RecycleBinId);
                Mandate.That(recycleBinEntity != null, x => new NullReferenceException("Could not find the Recycle bin entity with Id" + RecycleBinId + " in the repository"));

                var recycleBinNode = CreateTreeNode(RecycleBinId, queryStrings, "Recycle bin", Url.GetCurrentDashboardUrl());
                //TODO: check if bin is empty, if so render the bin_empty.png icon
                recycleBinNode.Icon = Url.Content(BackOfficeRequestContext.Application.Settings.RebelCmsFolders.DocTypeIconFolder + "/bin.png");
                
                //add the menu items
                recycleBinNode.AddMenuItem<EmptyRecycleBin>("emptyBinUrl", Url.GetEditorUrl("EmptyBin", null, EditorControllerId, BackOfficeRequestContext.RegisteredComponents, BackOfficeRequestContext.Application.Settings));
                recycleBinNode.AddEditorMenuItem<Permissions>(this, "permissionsUrl", "Permissions");
                recycleBinNode.AddMenuItem<Reload>();

                recycleBinNode.Style.AddCustom("recycle-bin");

                //check if it has children
                recycleBinNode.HasChildren = recycleBinEntity.RelationProxies.GetChildRelations(FixedRelationTypes.DefaultRelationType).Any();

                NodeCollection.Add(recycleBinNode);
            }
            

            
        }

    }
}