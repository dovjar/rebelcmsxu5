using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Trees.MenuItems;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Security.Model.Entities;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Trees
{
    public abstract class AbstractUserGroupTree : SupportsEditorTreeController
    {
        protected AbstractUserGroupTree(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        { }

        public abstract string ProviderGroupRoot { get; }

        public abstract HiveId VirtualRoot { get; }

        /// <summary>
        /// Customize root node
        /// </summary>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected override TreeNode CreateRootNode(FormCollection queryStrings)
        {
            var n = base.CreateRootNode(queryStrings);
            //add some menu items to the created root node
            n.AddEditorMenuItem<CreateItem>(this, "createUrl", "Create");
            n.AddMenuItem<Reload>();
            return n;
        }

        protected override UmbracoTreeResult GetTreeData(HiveId parentId, FormCollection queryStrings)
        {
            //if its the first level
            if (parentId == RootNodeId)
            {
                var hive = BackOfficeRequestContext.Application.Hive.GetReader<ISecurityStore>(
                    //BUG: this check is only a work around because the way that Hive currently works cannot return the 'real' id of the entity. SD.
                    parentId == RootNodeId
                        ? new Uri(ProviderGroupRoot)
                        : parentId.ToUri());

                Mandate.That(hive != null, x => new NotSupportedException("Could not find a hive provider for route: " + parentId.ToString(HiveIdFormatStyle.AsUri)));

                using (var uow = hive.CreateReadonly())
                {
                    //TODO: not sure how this is supposed to be casted to UserGroup
                    var items = uow.Repositories.GetChildren<UserGroup>(FixedRelationTypes.DefaultRelationType, VirtualRoot)
                        .OrderBy(x => x.Name)
                        .ToArray();

                    foreach (var treeNode in items.Select(userGroup =>
                                (TreeNode)CreateTreeNode(
                                    userGroup.Id,
                                    queryStrings,
                                    userGroup.Name,
                                    GetEditorUrl(userGroup.Id, queryStrings))))
                    {
                        treeNode.Icon = "tree-user-type";
                        treeNode.HasChildren = false;

                        //add the menu items
                        if (treeNode.Title != "Administrator")
                            treeNode.AddEditorMenuItem<Delete>(this, "deleteUrl", "Delete");

                        NodeCollection.Add(treeNode);
                    }
                }
            }
            else
            {
                throw new NotSupportedException("The User Group tree does not support more than 1 level");
            }

            return UmbracoTree();
        }

        protected override HiveId RootNodeId
        {
            get
            {
                //BUG: Because of the current way that Hive is (30/09/2011), Hive cannot return the real Id representation,
                // so in the meantime to be consistent so that tree syncing works properly, we need to lookup the root id 
                // from hive and use it's returned value as the root node id. SD.
                using (var uow = BackOfficeRequestContext.Application.Hive.OpenReader<ISecurityStore>(new Uri(ProviderGroupRoot)))
                {
                    var root = uow.Repositories.Get<TypedEntity>(VirtualRoot);
                    return root.Id;
                }

                //return FixedHiveIds.UserGroupVirtualRoot;
            }
        }

    }
}
