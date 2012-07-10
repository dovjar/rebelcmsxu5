using System;
using System.Linq;
using System.Web.Mvc;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model;
using Rebel.Cms.Web.Mvc;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Cms.Web.Mvc.ActionInvokers;
using Rebel.Cms.Web.Routing;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model;

namespace Rebel.Cms.Web.Surface
{
    /// <summary>
    /// The base controller that all Presentation Add-in controllers should inherit from
    /// </summary>
    [MergeModelStateToChildAction]
    public abstract class SurfaceController : Controller, IRequiresRoutableRequestContext
    {
        public IRoutableRequestContext RoutableRequestContext { get; set; }

        /// <summary>
        /// Useful for debugging
        /// </summary>
        public Guid InstanceId { get; private set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="routableRequestContext"></param>
        protected SurfaceController(IRoutableRequestContext routableRequestContext)
        {
            RoutableRequestContext = routableRequestContext;
            InstanceId = Guid.NewGuid();
        }

        /// <summary>
        /// Empty constructor, uses DependencyResolver to resolve the IRoutableRequestContext
        /// </summary>
        protected SurfaceController()
        {
            RoutableRequestContext = DependencyResolver.Current.GetService<IRoutableRequestContext>();
            InstanceId = Guid.NewGuid();
        }

        /// <summary>
        /// Redirects to the Rebel page with the given id
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        protected RedirectToRebelPageResult RedirectToRebelPage(HiveId pageId)
        {
            return new RedirectToRebelPageResult(pageId, RoutableRequestContext);
        }

        /// <summary>
        /// Redirects to the Rebel page with the given id
        /// </summary>
        /// <param name="pageEntity"></param>
        /// <returns></returns>
        protected RedirectToRebelPageResult RedirectToRebelPage(TypedEntity pageEntity)
        {
            return new RedirectToRebelPageResult(pageEntity, RoutableRequestContext);
        }

        /// <summary>
        /// Redirects to the currently rendered Rebel page
        /// </summary>
        /// <returns></returns>
        protected RedirectToRebelPageResult RedirectToCurrentRebelPage()
        {
            return new RedirectToRebelPageResult(CurrentPage, RoutableRequestContext);
        }

        /// <summary>
        /// Returns the currently rendered Rebel page
        /// </summary>
        /// <returns></returns>
        protected RebelPageResult CurrentRebelPage()
        {
            return new RebelPageResult();
        }

        /// <summary>
        /// Gets the current page.
        /// </summary>
        protected Content CurrentPage
        {
            get
            {
                if (!ControllerContext.RouteData.DataTokens.ContainsKey("rebel-route-def"))
                    throw new InvalidOperationException("Can only use " + typeof(RebelPageResult).Name + " in the context of an Http POST when using the BeginRebelForm helper");

                var routeDef = (RouteDefinition)ControllerContext.RouteData.DataTokens["rebel-route-def"];
                return routeDef.RenderModel.CurrentNode;
            }
        }

    }
}