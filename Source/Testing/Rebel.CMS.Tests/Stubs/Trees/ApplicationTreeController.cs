using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Cms.Web.Trees;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model.Constants;

namespace Rebel.Tests.Cms.Stubs.Trees
{
    /// <summary>
    /// A tree for testing routes when there are multiple controllers with the same name as plugins
    /// </summary>
    [Tree("5C0BB383-83A9-4A9C-9B66-691C46B88C11", "Application")]
    internal class ApplicationTreeController : TreeController
    {
        public ApplicationTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext) { }

        protected override RebelTreeResult GetTreeData(HiveId parentId, FormCollection queryStrings)
        {
            return RebelTree();

        }

        protected override HiveId RootNodeId
        {
            get { return FixedHiveIds.SystemRoot; }
        }
    }
}