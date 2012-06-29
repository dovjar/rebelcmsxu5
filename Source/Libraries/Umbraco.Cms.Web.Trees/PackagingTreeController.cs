using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Packaging;
using Umbraco.Cms.Web.Trees.MenuItems;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.IO;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Trees
{
    [Tree("FD72E12C-2FF5-4B51-8F60-F3B57E06080F", "Packages")]
    [UmbracoTree]
    public class PackagingTreeController : SupportsEditorTreeController
    {

        public PackagingTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
        }

        /// <summary>
        /// Returns a unique tree root node id
        /// </summary>
        /// <remarks>
        /// We are assigning a static unique id to this tree in order to facilitate tree node syncing
        /// </remarks>
        protected override HiveId RootNodeId
        {
            get { return new HiveId(FixedSchemaTypes.SystemRoot, null, new HiveIdValue(new Guid("802282F2-134E-4165-82B5-6DB2AFDDD135"))); }
        }

        public override Guid EditorControllerId
        {
            get { return new Guid(CorePluginConstants.PackagingEditorControllerId); }
        }

        protected override UmbracoTreeResult GetTreeData(HiveId parentId, FormCollection queryStrings)
        {
            if (parentId == RootNodeId)
            {
                foreach (var node in PackageTreeNodes
                    .Select(x =>
                            CreateTreeNode(
                                x.Item1,
                                queryStrings,
                                x.Item2,
                                !string.IsNullOrWhiteSpace(x.Item3)
                                    ? Url.GetEditorUrl(x.Item3, null, EditorControllerId, BackOfficeRequestContext.RegisteredComponents, BackOfficeRequestContext.Application.Settings)
                                    : Url.GetCurrentDashboardUrl(),
                                x.Item3,
                                false,
                                "tree-repository")))
                {
                    if ((int)node.HiveId.Value.Value == 3)
                    {
                        node.AddEditorMenuItem<CreateItem>(this, "createUrl", "Create");
                        node.AddMenuItem<Reload>();

                        node.HasChildren = true;
                    }

                    NodeCollection.Add(node);
                }
            }
            else
            {
                if((int)parentId.Value.Value == 3)
                {
                    using(var uow = BackOfficeRequestContext.Application.Hive.OpenReader<IFileStore>(new Uri("storage://created-packages")))
                    {
                        var root = uow.Repositories.Get<File>(new HiveId("/"));
                        var defs = uow.Repositories.GetChildRelations(root, FixedRelationTypes.DefaultRelationType)
                            .Select(x => PackageBuilderHelper.GetPackageDefinitionById(uow, x.DestinationId))
                            .Where(x => x != null)
                            .ToList();

                        foreach (var def in defs)
                        {
                            var n = CreateTreeNode(
                                def.Id,
                                queryStrings,
                                def.Name,
                                Url.GetEditorUrl("Edit", def.Id, EditorControllerId, BackOfficeRequestContext.RegisteredComponents, BackOfficeRequestContext.Application.Settings),
                                "Edit",
                                false,
                                "tree-package");


                            n.AddEditorMenuItem<Delete>(this, "deleteUrl", "Delete");

                            NodeCollection.Add(n);
                        }
                    }
                }
                else
                {
                    throw new NotImplementedException("The Packaging tree does not support more than 1 level");
                }
            }

            return UmbracoTree();
        }

        /// <summary>
        /// Represents the list of nodes for the tree. 
        /// </summary>
        /// <remarks>
        /// Each item in the Tuple represents: the node ID, the name, the controller Action name
        /// </remarks>
        private static IEnumerable<Tuple<HiveId, string, string>> PackageTreeNodes
        {
            get
            {
                return new List<Tuple<HiveId, string, string>>
                    {
                        //new Tuple<HiveId, string, string>((HiveId)1, "Package repository", "PublicRepository"), 
                        new Tuple<HiveId, string, string>(new HiveId(3), "Created packages", ""),
                        new Tuple<HiveId, string, string>(new HiveId(2), "Local packages", "LocalRepository")                              
                    };
            }
        }


    }
}
