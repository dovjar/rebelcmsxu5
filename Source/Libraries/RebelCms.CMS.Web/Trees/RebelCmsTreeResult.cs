using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

using System.Web;
using RebelCms.Cms.Web.Model.BackOffice.Trees;

namespace RebelCms.Cms.Web.Trees
{
    /// <summary>
    /// Returns a result for an RebelCms tree
    /// </summary>
    public class RebelCmsTreeResult : ContentResult
    {
        /// <summary>
        /// Constructs the tree result with the pre compiled TreeNodeCollection, also checks for any JavaScript files that 
        /// need to be added via ClientDependency that have been specified on menu items.
        /// </summary>
        /// <param name="nodeCollection"></param>
        /// <param name="controller">The current ControllerContext</param>
        public RebelCmsTreeResult(TreeNodeCollection nodeCollection, ControllerContext controller)
        {
            Content = nodeCollection.Serialize();
            ContentType = "application/json";
            ContentEncoding = Encoding.UTF8;

            NodeCollection = nodeCollection;
        }

        /// <summary>
        /// Returns a non-mutable collection of nodes
        /// </summary>
        public IEnumerable<TreeNode> NodeCollection { get; private set; }

    }
}
