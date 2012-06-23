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
    [Tree("1296D4BA-459F-4302-B35C-70B47E6F8117", "Content")]
    internal class MediaTreeController : TreeController
    {
        public MediaTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext) { }

        protected override RebelCmsTreeResult GetTreeData(HiveId parentId, FormCollection queryStrings)
        {
            return RebelCmsTree();

        }

        protected override HiveId RootNodeId
        {
            get { return FixedHiveIds.MediaVirtualRoot; }
        }
    }
}