using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Cms.Web.Mvc.Controllers.BackOffice;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model.Constants;

namespace Rebel.Cms.Web.Trees
{
    /// <summary>
    /// The base controller for all tree requests
    /// </summary>    
    public abstract class TreeController : SecuredBackOfficeController
    {
        
        #region Static Methods
        /// <summary>
        /// Returns a controller name based on tree type
        /// </summary>
        /// <param name="treeType"></param>
        /// <returns></returns>
        /// <remarks>
        /// Tree contrllers are always suffixed with "Tree"
        /// </remarks>
        public static string GetControllerName(string treeType)
        {
            return string.Concat(treeType, "Tree");
        }
 
        #endregion

        protected TreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {

            //Locate the tree attribute
            var treeAttributes = GetType()
                .GetCustomAttributes(typeof(TreeAttribute), false)
                .OfType<TreeAttribute>();

            if (!treeAttributes.Any())
            {
                throw new InvalidOperationException("The Tree controller is missing the " + typeof(TreeAttribute).FullName + " attribute");
            }

            //assign the properties of this object to those of the metadata attribute
            var attr = treeAttributes.First();
            TreeId = attr.Id;
            NodeDisplayName = attr.TreeTitle;
            NodeCollection = new TreeNodeCollection();
        }

        /// <summary>
        /// The method called to render the contents of the tree structure
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="queryStrings">
        /// All of the query string parameters passed from jsTree
        /// </param>
        /// <returns>JSON markup for jsTree</returns>        
        /// <remarks>
        /// We are allowing an arbitrary number of query strings to be pased in so that developers are able to persist custom data from the front-end
        /// to the back end to be used in the query for model data.
        /// </remarks>
        protected abstract RebelTreeResult GetTreeData(HiveId parentId, FormCollection queryStrings);

        /// <summary>
        /// Returns the root node for the tree
        /// </summary>
        protected abstract HiveId RootNodeId { get; }

        /// <summary>
        /// Returns the ID defined for the tree
        /// </summary>
        /// <remarks>
        /// The ID is derived from the attributed defined on the tree controller
        /// </remarks>
        public Guid TreeId { get; private set; }

        /// <summary>
        /// The name to display on the root node
        /// </summary>
        public string NodeDisplayName { get; private set; }
        
        /// <summary>
        /// The action called to render the contents of the tree structure
        /// </summary>
        /// <param name="id"></param>
        /// <param name="querystrings">
        /// All of the query string parameters passed from jsTree
        /// </param>
        /// <returns>JSON markup for jsTree</returns>        
        /// <remarks>
        /// We are allowing an arbitrary number of query strings to be pased in so that developers are able to persist custom data from the front-end
        /// to the back end to be used in the query for model data.
        /// </remarks>
        [QueryStringFilter("querystrings")]
        [HttpGet]
        public virtual RebelTreeResult Index(HiveId id, FormCollection querystrings)
        {
            //if the id is empty, then set it as the root node/self id
            if (id.IsNullValueOrEmpty())
            {
                id = new HiveId(TreeId);
            }

            //if its the root node, render it otherwise render normal nodes
            return AddRootNodeToCollection(id, querystrings)
                ? RebelTree() 
                : GetTreeData(id, querystrings);
        }

        /// <summary>
        /// Helper method to create a root model for a tree
        /// </summary>
        /// <returns></returns>
        protected virtual TreeNode CreateRootNode(FormCollection queryStrings)
        {
            var jsonUrl = Url.GetTreeUrl(GetType(), RootNodeId, queryStrings);
            var isDialog = queryStrings.GetValue<bool>(TreeQueryStringParameters.DialogMode);
            var node = new TreeNode(RootNodeId, BackOfficeRequestContext.RegisteredComponents.MenuItems, jsonUrl)
                {
                    HasChildren = true,
                    EditorUrl = queryStrings.HasKey(TreeQueryStringParameters.OnNodeClick) //has a node click handler?
                                    ? queryStrings.Get(TreeQueryStringParameters.OnNodeClick) //return node click handler
                                    : isDialog //is in dialog mode without a click handler ?
                                          ? "#" //return empty string, otherwise, return an editor URL:
                                          : Url.GetCurrentDashboardUrl(),
                    Title = NodeDisplayName
                };

            //add the tree id to the root
            node.AdditionalData.Add("treeId", TreeId.ToString("N"));
            
            //add the tree-root css class
            node.Style.AddCustom("tree-root");

            //node.AdditionalData.Add("id", node.HiveId.ToString());
            //node.AdditionalData.Add("title", node.Title);

            AddQueryStringsToAdditionalData(node, queryStrings);

            //check if the tree is searchable and add that to the meta data as well
            if (this is ISearchableTree)
            {
                node.AdditionalData.Add("searchable", "true");
            }

            return node;
        }

        /// <summary>
        /// The AdditionalData of a node is always populated with the query string data, this method performs this
        /// operation and ensures that special values are not inserted or that duplicate keys are not added.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="queryStrings"></param>
        protected virtual void AddQueryStringsToAdditionalData(TreeNode node, FormCollection queryStrings)
        {
            // Add additional data, ensure treeId isn't added as we've already done that
            foreach (var key in queryStrings.AllKeys
                .Except(new[] { "treeId" })
                .Where(key => !node.AdditionalData.ContainsKey(key)))
            {
                node.AdditionalData.Add(key, queryStrings[key]);
            }
        }

        /// <summary>
        /// Checks if the node Id is the root node and if so creates the root node and appends it to the 
        /// NodeCollection based on the standard tree parameters
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        /// <remarks>
        /// This method ensure that all of the correct meta data is set for the root node so that the Rebel application works
        /// as expected. Meta data such as 'treeId' and 'searchable'
        /// </remarks>
        protected bool AddRootNodeToCollection(HiveId id, FormCollection queryStrings)
        {
            Mandate.ParameterNotEmpty(id, "id");
            Mandate.ParameterCondition(!id.IsNullValueOrEmpty(), "id");
            
            //if its the root model
            if (id.Equals(TreeId))
            {
                //get the root model
                var rootNode = CreateRootNode(queryStrings);
             
                NodeCollection.Add(rootNode);

                return true;
            }

            return false;
        }

        #region Create TreeNode methods

        /// <summary>
        /// Helper method to create tree nodes and automatically generate the json url
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <param name="title"></param>
        /// <param name="editorUrl"></param>
        /// <returns></returns>
        public TreeNode CreateTreeNode(HiveId id, FormCollection queryStrings, string title, string editorUrl)
        {
            var jsonUrl = Url.GetTreeUrl(GetType(), id, queryStrings);
            var node = new TreeNode(id, BackOfficeRequestContext.RegisteredComponents.MenuItems, jsonUrl) { Title = title, EditorUrl = editorUrl };
            return node;
        }

        /// <summary>
        /// Helper method to create tree nodes and automatically generate the json url
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <param name="title"></param>
        /// <param name="editorUrl"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public TreeNode CreateTreeNode(HiveId id, FormCollection queryStrings, string title, string editorUrl, string action)
        {
            var jsonUrl = Url.GetTreeUrl(action, GetType(), id, queryStrings);
            var node = new TreeNode(id, BackOfficeRequestContext.RegisteredComponents.MenuItems, jsonUrl) { Title = title, EditorUrl = editorUrl };
            return node;
        }

        /// <summary>
        /// Helper method to create tree nodes and automatically generate the json url
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <param name="title"></param>
        /// <param name="editorUrl"></param>
        /// <param name="action"></param>
        /// <param name="icon"></param>
        /// <returns></returns>
        public TreeNode CreateTreeNode(HiveId id, FormCollection queryStrings, string title, string editorUrl, string action, string icon)
        {
            var jsonUrl = Url.GetTreeUrl(action, GetType(), id, queryStrings);
            var node = new TreeNode(id, BackOfficeRequestContext.RegisteredComponents.MenuItems, jsonUrl) { Title = title, EditorUrl = editorUrl };
            return node;
        }

        /// <summary>
        /// Helper method to create tree nodes and automatically generate the json url
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <param name="title"></param>
        /// <param name="editorUrl"></param>
        /// <param name="action"></param>
        /// <param name="hasChildren"></param>
        /// <returns></returns>
        public TreeNode CreateTreeNode(HiveId id, FormCollection queryStrings, string title, string editorUrl, string action, bool hasChildren)
        {
            var treeNode = CreateTreeNode(id, queryStrings, title, editorUrl, action);
            treeNode.HasChildren = hasChildren;
            return treeNode;
        }

        /// <summary>
        /// Helper method to create tree nodes and automatically generate the json url
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <param name="title"></param>
        /// <param name="editorUrl"></param>
        /// <param name="action"></param>
        /// <param name="hasChildren"></param>
        /// <param name="icon"></param>
        /// <returns></returns>
        public TreeNode CreateTreeNode(HiveId id, FormCollection queryStrings, string title, string editorUrl, string action, bool hasChildren, string icon)
        {
            var treeNode = CreateTreeNode(id, queryStrings, title, editorUrl, action);
            treeNode.HasChildren = hasChildren;
            treeNode.Icon = icon;
            return treeNode;
        }

        /// <summary>
        /// Helper method to create tree nodes and automatically generate the json url
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <param name="title"></param>
        /// <param name="editorUrl"></param>
        /// <param name="hasChildren"></param>
        /// <returns></returns>
        public TreeNode CreateTreeNode(HiveId id, FormCollection queryStrings, string title, string editorUrl, bool hasChildren)
        {
            var treeNode = CreateTreeNode(id, queryStrings, title, editorUrl);
            treeNode.HasChildren = hasChildren;
            return treeNode;
        }

        /// <summary>
        /// Helper method to create tree nodes and automatically generate the json url
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <param name="title"></param>
        /// <param name="editorUrl"></param>
        /// <param name="hasChildren"></param>
        /// <param name="icon"></param>
        /// <returns></returns>
        public TreeNode CreateTreeNode(HiveId id, FormCollection queryStrings, string title, string editorUrl, bool hasChildren, string icon)
        {
            var treeNode = CreateTreeNode(id, queryStrings, title, editorUrl);
            treeNode.HasChildren = hasChildren;
            treeNode.Icon = icon;
            return treeNode;
        } 

        #endregion

        /// <summary>
        /// The tree name based on the controller type so that everything is based on naming conventions
        /// </summary>
        public string TreeType
        {
            get
            {
                var name = GetType().Name;
                return name.Substring(0, name.LastIndexOf("TreeController"));
            }
        }

        

        /// <summary>
        /// The model collection for trees to add nodes to
        /// </summary>
        protected TreeNodeCollection NodeCollection { get; private set; }

        /// <summary>
        /// Used to return the serialized JSON for the tree
        /// </summary>
        /// <returns></returns>
        protected RebelTreeResult RebelTree()
        {
            return new RebelTreeResult(NodeCollection, ControllerContext);
        }
        
    }
}
