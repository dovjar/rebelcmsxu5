using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Editors;
using RebelCms.Cms.Web.Model.BackOffice.Trees;
using RebelCms.Cms.Web.Trees.MenuItems;
using RebelCms.Framework;

using RebelCms.Framework.Persistence;
using RebelCms.Framework.Persistence.Model.Attribution.MetaData;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Persistence.Model;
using RebelCms.Framework.Persistence.Model.Constants.Entities;
using RebelCms.Hive;
using RebelCms.Hive.RepositoryTypes;


namespace RebelCms.Cms.Web.Trees
{
    /// <summary>
    /// Tree controller to render out the data types
    /// </summary>
    [Tree(CorePluginConstants.UserTreeControllerId, "Users")]
    [RebelCmsTree]
    public class UserTreeController : SupportsEditorTreeController
    {

        public UserTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
        }       

        /// <summary>
        /// Defines the data type editor
        /// </summary>
        public override Guid EditorControllerId
        {
            get { return new Guid(CorePluginConstants.UserEditorControllerId); }
        }

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

        protected override RebelCmsTreeResult GetTreeData(HiveId parentId, FormCollection queryStrings)
        {
            //if its the first level
            if (parentId == RootNodeId)
            {
                var hive = BackOfficeRequestContext.Application.Hive.GetReader<ISecurityStore>(
                    //BUG: this check is only a work around because the way that Hive currently works cannot return the 'real' id of the entity. SD.
                    (parentId == RootNodeId)
                        ? new Uri("security://users")
                        : parentId.ToUri());

                Mandate.That(hive != null, x => new NotSupportedException("Could not find a hive provider for route: " + parentId.ToString(HiveIdFormatStyle.AsUri)));

                using (var uow = hive.CreateReadonly())
                {
                    var items = uow.Repositories.GetEntityByRelationType<User>(FixedRelationTypes.DefaultRelationType, FixedHiveIds.UserVirtualRoot)
                        .OrderBy(x => x.Name)
                        .ToArray();

                    foreach (var treeNode in items.Select(user =>
                                (TreeNode)CreateTreeNode(
                                    user.Id,
                                    queryStrings,
                                    user.Name,
                                    Url.GetEditorUrl(user.Id, EditorControllerId, BackOfficeRequestContext.RegisteredComponents, BackOfficeRequestContext.Application.Settings))))
                    {
                        treeNode.Icon = "tree-user";
                        treeNode.HasChildren = false;

                        //add the menu items
                        treeNode.AddEditorMenuItem<Delete>(this, "deleteUrl", "Delete");

                        NodeCollection.Add(treeNode);
                    }
                }
            }
            else
            {
                throw new NotSupportedException("The User tree does not support more than 1 level");
            }

            return RebelCmsTree();
        }

        protected override HiveId RootNodeId
        {
            get
            {
                ////BUG: Because of the current way that Hive is (30/09/2011), Hive cannot return the real Id representation,
                //// so in the meantime to be consistent so that tree syncing works properly, we need to lookup the root id 
                //// from hive and use it's returned value as the root node id. SD.
                //using (var uow = BackOfficeRequestContext.Application.Hive.OpenReader<ISecurityStore>(new Uri("security://users")))
                //{
                //    var root = uow.Repositories.Get(FixedHiveIds.UserVirtualRoot);
                //    return root.Id;
                //}

                return new HiveId(new Uri("security://users"), "", FixedHiveIds.UserVirtualRoot.Value);

            }
        }

    }
}
