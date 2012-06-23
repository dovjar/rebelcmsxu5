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
    /// A tree to test duplicate controller names as plugins
    /// </summary>
    [Tree("1C841BC7-915C-4362-B844-A27A0A3F4399", "TreeController")]
    internal class ContentTreeController : TreeController
    {
        public ContentTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext) { }

        protected override RebelCmsTreeResult GetTreeData(HiveId parentId, FormCollection queryStrings)
        {
            return RebelCmsTree();
        }

        protected override HiveId RootNodeId
        {
            get { return FixedHiveIds.ContentVirtualRoot; }
        }
    }
}