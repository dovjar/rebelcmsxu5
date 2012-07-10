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
    /// A tree to test duplicate controller names as plugins
    /// </summary>
    [Tree("1C841BC7-915C-4362-B844-A27A0A3F4399", "TreeController")]
    internal class ContentTreeController : TreeController
    {
        public ContentTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext) { }

        protected override RebelTreeResult GetTreeData(HiveId parentId, FormCollection queryStrings)
        {
            return RebelTree();
        }

        protected override HiveId RootNodeId
        {
            get { return FixedHiveIds.ContentVirtualRoot; }
        }
    }
}