using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Rebel.Cms.Web.Caching;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.EmbeddedViewEngine;
using Rebel.Cms.Web.Model;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Cms.Web.Mvc.ActionInvokers;
using Rebel.Cms.Web.Routing;
using Rebel.Framework;
using Rebel.Framework.Diagnostics;
using Rebel.Framework.Persistence.Model;
using MSMvc = System.Web.Mvc;


namespace Rebel.Cms.Web.Mvc.Controllers
{
    /// <summary>
    /// The default controller for Rebel front-end request
    /// </summary>
    [InstalledFilter]
    [Internationalize]
    public class RebelController : Controller, IRequiresRoutableRequestContext
    {
        /// <summary>
        /// Constructor initializes custom action invoker
        /// </summary>
        public RebelController()
        {
            //this could be done by IoC but we really don't want people to have to create
            //the custom constructor each time they want to create a controller that extends this one.
            ActionInvoker = new RebelActionInvoker(RoutableRequestContext);
            RoutableRequestContext = DependencyResolver.Current.GetService<IRoutableRequestContext>();
        }

        /// <summary>
        /// Constructor initializes custom action invoker
        /// </summary>
        public RebelController(IRoutableRequestContext routableRequestContext)
        {
            ActionInvoker = new RebelActionInvoker(RoutableRequestContext);
            RoutableRequestContext = routableRequestContext;
        }

        #region IRequiresRoutableRequestContext Members

        public IRoutableRequestContext RoutableRequestContext { get; set; }

        #endregion

        #region Static methods

        /// <summary>
        /// Return the ID of an controller plugin
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="editorControllerType"></param>
        /// <returns></returns>
        [Obsolete("Use the ControllerExtensions.GetControllerById method instead")]
        public static Guid GetControllerId<T>(Type editorControllerType)
            where T : PluginAttribute
        {
            return ControllerExtensions.GetControllerId<T>(editorControllerType);
        }

        /// <summary>
        /// Return the controller name from the controller type
        /// </summary>
        /// <param name="controllerType"></param>
        /// <returns></returns>
        [Obsolete("Use the ControllerExtensions.GetControllerName method instead")]
        public static string GetControllerName(Type controllerType)
        {
            return ControllerExtensions.GetControllerName(controllerType);
        }

        /// <summary>
        /// Return the controller name from the controller type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <remarks></remarks>
        [Obsolete("Use the ControllerExtensions.GetControllerName method instead")]
        public static string GetControllerName<T>()
        {
            return ControllerExtensions.GetControllerName<T>();
        }

        #endregion

        /// <summary>
        /// The default action to render the template/view
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This will check if the view for the template for the IRoutableItem exists, if it doesn't will render the default 'Index' view.
        /// </remarks>        
        [PublicAccessAuthorize]
        [OutputCache(CacheProfile = "rebel-default")]
        [EnableCompression]
        public virtual ActionResult Index(IRebelRenderModel model)
        {
            if (model.CurrentNode == null) return new HttpNotFoundResult();

            var contentCacheManager = new ContentCacheManager(RoutableRequestContext.Application, this);
            var content = contentCacheManager.RenderContent(model);

            if (string.IsNullOrEmpty(content.Key))
            {
                return View(
                    EmbeddedViewPath.Create("Rebel.Cms.Web.EmbeddedViews.Views.TemplateNotFound.cshtml,Rebel.Cms.Web"),
                    model.CurrentNode);
            }

            return Content(content.Value, content.Key);
        }

        protected override void Dispose(bool disposing)
        {
            LogHelper.TraceIfEnabled<RebelController>("Controller being disposed, calling scope cleanup: {0}",
                                                      () => RoutableRequestContext.Application != null);
            if (RoutableRequestContext.Application != null)
                RoutableRequestContext.Application.FrameworkContext.ScopedFinalizer.FinalizeScope();
            base.Dispose(disposing);
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
            //validate that the current page execution is not being handled by the normal rebel routing system
            if (!ControllerContext.RouteData.DataTokens.ContainsKey("rebel-route-def"))
            {
                throw new InvalidOperationException("Can only use " + typeof (RebelPageResult).Name +
                                                    " in the context of an Http POST when using the BeginRebelForm helper");
            }

            var routeDef = (RouteDefinition) ControllerContext.RouteData.DataTokens["rebel-route-def"];
            return new RedirectToRebelPageResult(routeDef.RenderModel.CurrentNode, RoutableRequestContext);
        }
    }
}