using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Rebel.Cms.Web.Context;
using Rebel.Framework;

namespace Rebel.Cms.Web.Trees
{
    /// <summary>
    /// An abstract tree that supports menu actions with an editor
    /// </summary>
    public abstract class SupportsEditorTreeController : TreeController
    {
        protected SupportsEditorTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
        }

        /// <summary>
        /// The ID of the editor controller
        /// </summary>
        public abstract Guid EditorControllerId { get; }

        /// <summary>
        /// Return the editor URL for the currrent node depending on the data found in the query strings
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        /// <remarks>
        /// This checks if the tree there is a OnNodeClick handler assigned, if so, it assigns it,
        /// otherwise it checks if the tree is in DialogMode, if it is then it returns an empty handler, otherwise
        /// it sets the Url to the editor's url. 
        /// </remarks>
        public string GetEditorUrl(HiveId id, FormCollection queryStrings)
        {
            Mandate.ParameterNotEmpty(id, "id");
            Mandate.ParameterNotNull(queryStrings, "queryStrings");
            Mandate.That<NullReferenceException>(Url != null);

            var isDialog = queryStrings.GetValue<bool>(TreeQueryStringParameters.DialogMode);
            return queryStrings.HasKey(TreeQueryStringParameters.OnNodeClick) //has a node click handler?
                       ? queryStrings.Get(TreeQueryStringParameters.OnNodeClick) //return node click handler
                       : isDialog //is in dialog mode without a click handler ?
                             ? "#" //return empty string, otherwise, return an editor URL:
                             : Url.GetEditorUrl(
                                 id,
                                 EditorControllerId,
                                 BackOfficeRequestContext.RegisteredComponents,
                                 BackOfficeRequestContext.Application.Settings);
        }

    }

}