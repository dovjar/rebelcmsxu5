using System;
using System.Collections.Generic;
using System.Web.Mvc;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Model.BackOffice.Trees;
using RebelCms.Cms.Web.Trees;
using RebelCms.Framework;
using RebelCms.Framework.Persistence.Model.Constants;

namespace RebelCms.Tests.Cms.Stubs.Trees
{
    /// <summary>
    /// A tree for testing routes when there are multiple controllers with the same name as plugins
    /// </summary>
    [Tree("5C0BB383-83A9-4A9C-9B66-691C46B88C11", "Application")]
    internal class ApplicationTreeController : TreeController
    {
        public ApplicationTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext) { }

        protected override RebelCmsTreeResult GetTreeData(HiveId parentId, FormCollection queryStrings)
        {
            return RebelCmsTree();

        }

        protected override HiveId RootNodeId
        {
            get { return FixedHiveIds.SystemRoot; }
        }
    }
}