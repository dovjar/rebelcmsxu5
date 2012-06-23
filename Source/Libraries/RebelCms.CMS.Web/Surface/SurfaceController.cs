using System;
using System.Linq;
using System.Web.Mvc;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Mvc;
using RebelCms.Cms.Web.Routing;
using RebelCms.Framework;
using RebelCms.Framework.Persistence.Model;

namespace RebelCms.Cms.Web.Surface
{
    /// <summary>
    /// The base controller that all Presentation Add-in controllers should inherit from
    /// </summary>
    public abstract class SurfaceController : Controller, IRequiresRoutableRequestContext
    {
        public IRoutableRequestContext RoutableRequestContext { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="routableRequestContext"></param>
        protected SurfaceController(IRoutableRequestContext routableRequestContext)
        {
            RoutableRequestContext = routableRequestContext;            
        }

        /// <summary>
        /// Empty constructor, uses DependencyResolver to resolve the IRoutableRequestContext
        /// </summary>
        protected SurfaceController()
        {
            RoutableRequestContext = DependencyResolver.Current.GetService<IRoutableRequestContext>();
        }
       
        /// <summary>
        /// Redirects to the RebelCms page with the given id
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        protected RedirectToRebelCmsPageResult RedirectToRebelCmsPage(HiveId pageId)
        {            
            return new RedirectToRebelCmsPageResult(pageId, RoutableRequestContext);
        }

        /// <summary>
        /// Redirects to the RebelCms page with the given id
        /// </summary>
        /// <param name="pageEntity"></param>
        /// <returns></returns>
        protected RedirectToRebelCmsPageResult RedirectToRebelCmsPage(TypedEntity pageEntity)
        {
            return new RedirectToRebelCmsPageResult(pageEntity, RoutableRequestContext);
        }

        /// <summary>
        /// Redirects to the currently rendered RebelCms page
        /// </summary>
        /// <returns></returns>
        protected RedirectToRebelCmsPageResult RedirectToCurrentRebelCmsPage()
        {
            //validate that the current page execution is not being handled by the normal RebelCms routing system
            if (!ControllerContext.RouteData.DataTokens.ContainsKey("RebelCms-route-def"))
            {
                throw new InvalidOperationException("Can only use " + typeof(RebelCmsPageResult).Name + " in the context of an Http POST when using the BeginRebelCmsForm helper");
            }

            var routeDef = (RouteDefinition)ControllerContext.RouteData.DataTokens["RebelCms-route-def"];
            return new RedirectToRebelCmsPageResult(routeDef.RenderModel.CurrentNode, RoutableRequestContext);
        }

        /// <summary>
        /// Returns the currently rendered RebelCms page
        /// </summary>
        /// <returns></returns>
        protected RebelCmsPageResult CurrentRebelCmsPage()
        {
            return new RebelCmsPageResult();
        }

    }
}
