using System;
using System.Collections.Generic;
using System.Linq;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Editors;
using System.Web.Mvc;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Cms.Web.Trees.MenuItems;
using Rebel.Framework;
using Rebel.Framework.Persistence;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Hive;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Cms.Web.Trees
{
    [Tree(CorePluginConstants.DocumentTypeTreeControllerId, "Document types")]
    [RebelTree]
    public class DocumentTypeTreeController : SupportsEditorTreeController
    {

        public DocumentTypeTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
        }

        /// <summary>
        /// Returns the SystemRoot node as the start node for this tree
        /// </summary>
        protected override HiveId RootNodeId
        {
            get { return FixedHiveIds.ContentRootSchema; }
        }


        /// <summary>
        /// Defines the document type editor
        /// </summary>
        public override Guid EditorControllerId
        {
            get { return new Guid(CorePluginConstants.DocumentTypeEditorControllerId); }
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
            n.AddEditorMenuItem<CreateItem>(this, "createUrl", "CreateNew");
            n.AddMenuItem<Reload>();
            return n;
        }

        protected override RebelTreeResult GetTreeData(HiveId parentId, FormCollection queryStrings)
        {
            using (var uow = BackOfficeRequestContext.Application.Hive.OpenReader<IContentStore>())
            {
                var items = uow.Repositories.Schemas.GetChildren<EntitySchema>(FixedRelationTypes.DefaultRelationType, parentId)
                    //don't include the 'special' schemas
                    .Where(x => !x.Id.IsSystem())
                    .OrderBy(x => x.Name.Value);

                foreach (var treeNode in
                    items
                        .Select(dt =>
                                CreateTreeNode(
                                    dt.Id,
                                    queryStrings,
                                    dt.Name,
                                    GetEditorUrl(dt.Id, queryStrings),
                                    dt.RelationProxies.GetChildRelations(FixedRelationTypes.DefaultRelationType).Any(),
                                    "tree-data-type")))
                {

                    //add the menu items
                    treeNode.AddEditorMenuItem<CreateItem>(this, "createUrl", "CreateNew");
                    treeNode.AddEditorMenuItem<Delete>(this, "deleteUrl", "Delete");
                    treeNode.AddMenuItem<Reload>();

                    NodeCollection.Add(treeNode);
                }
            }

            return RebelTree();
        }




    }
}
