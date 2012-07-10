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
    [Tree("1296D4BA-459F-4302-B35C-70B47E6F8117", "Content")]
    internal class MediaTreeController : TreeController
    {
        public MediaTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext) { }

        protected override RebelTreeResult GetTreeData(HiveId parentId, FormCollection queryStrings)
        {
            return RebelTree();

        }

        protected override HiveId RootNodeId
        {
            get { return FixedHiveIds.MediaVirtualRoot; }
        }
    }
}