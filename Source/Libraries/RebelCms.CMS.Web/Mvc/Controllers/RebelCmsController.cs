using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.EmbeddedViewEngine;
using RebelCms.Cms.Web.Model;
using RebelCms.Cms.Web.Mvc.ActionFilters;
using RebelCms.Cms.Web.Mvc.ActionInvokers;
using RebelCms.Cms.Web.Mvc.Metadata;
using RebelCms.Cms.Web.Routing;
using RebelCms.Framework;
using RebelCms.Framework.Diagnostics;
using RebelCms.Framework.Persistence.Model;
using RebelCms.Hive;
using RebelCms.Hive.RepositoryTypes;
using File = RebelCms.Framework.Persistence.Model.IO.File;
using MSMvc = System.Web.Mvc;

namespace RebelCms.Cms.Web.Mvc.Controllers
{

    /// <summary>
    /// The default controller for RebelCms front-end request
    /// </summary>
    [InstalledFilter]
    [OutputCache(CacheProfile = "RebelCms-default")]
    public class RebelCmsController : Controller, IRequiresRoutableRequestContext
    {
        public IRoutableRequestContext RoutableRequestContext { get; set; }

        #region Static methods

        /// <summary>
        /// Return the ID of an controller plugin
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="editorControllerType"></param>
        /// <returns></returns>
        public static Guid GetControllerId<T>(Type editorControllerType)
            where T : PluginAttribute
        {
            //Locate the editor attribute
            var editorAttribute = editorControllerType
                .GetCustomAttributes(typeof(T), false)
                .OfType<T>();
            if (!editorAttribute.Any()) throw new InvalidOperationException("The controller plugin type is missing the " + typeof(T).FullName + " attribute");
            var attr = editorAttribute.First();
            return attr.Id;
        }

        /// <summary>
        /// Return the controller name from the controller type
        /// </summary>
        /// <param name="controllerType"></param>
        /// <returns></returns>
        public static string GetControllerName(Type controllerType)
        {
            return controllerType.Name.Substring(0, controllerType.Name.LastIndexOf("Controller"));
        }

        /// <summary>
        /// Return the controller name from the controller type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string GetControllerName<T>()
        {
            return GetControllerName(typeof (T));
        }
        #endregion

        /// <summary>
        /// Constructor initializes custom action invoker
        /// </summary>
        public RebelCmsController()
        {
            //this could be done by IoC but we really don't want people to have to create
            //the custom constructor each time they want to create a controller that extends this one.
            ActionInvoker = new RebelCmsActionInvoker(RoutableRequestContext);
            RoutableRequestContext = DependencyResolver.Current.GetService<IRoutableRequestContext>();
        }

        /// <summary>
        /// Constructor initializes custom action invoker
        /// </summary>
        public RebelCmsController(IRoutableRequestContext routableRequestContext)
        {
            ActionInvoker = new RebelCmsActionInvoker(RoutableRequestContext);
            RoutableRequestContext = routableRequestContext;
        }

        /// <summary>
        /// The default action to render the template/view
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This will check if the view for the template for the IRoutableItem exists, if it doesn't will render the default 'Index' view.
        /// </remarks>
        [Internationalize]
        public virtual ActionResult Index(IRebelCmsRenderModel model)
        {
            if (model.CurrentNode == null) return new HttpNotFoundResult();

            using (var uow = RoutableRequestContext.Application.Hive.OpenReader<IFileStore>(new Uri("storage://templates")))
            {
                var templateFile = model.CurrentNode.CurrentTemplate != null
                                   ? uow.Repositories.Get<File>(model.CurrentNode.CurrentTemplate.Id)
                                   : null;

                if (templateFile != null)
                {
                    //try to find the view based on all engines registered.
                    var view = MSMvc.ViewEngines.Engines.FindView(ControllerContext, 
                        Path.GetFileNameWithoutExtension(templateFile.RootedPath), "");

                    if (view.View != null)
                    {
                        return View(view.View, model.CurrentNode);
                    }
                }

                //return the compiled default view!
                //TODO: Remove magic strings
                return View(
                    EmbeddedViewPath.Create("RebelCms.Cms.Web.EmbeddedViews.Views.TemplateNotFound.cshtml,RebelCms.Cms.Web"),
                    model.CurrentNode);
            }

            
        }

        protected override void Dispose(bool disposing)
        {
            LogHelper.TraceIfEnabled<RebelCmsController>("Controller being disposed, calling scope cleanup: {0}", () => RoutableRequestContext.Application != null);
            if (RoutableRequestContext.Application != null)
                RoutableRequestContext.Application.FrameworkContext.ScopedFinalizer.FinalizeScope();
            base.Dispose(disposing);
        }

        /// <summary>
        /// Returns the currently rendered RebelCms page
        /// </summary>
        /// <returns></returns>
        protected RebelCmsPageResult CurrentRebelCmsPage()
        {
            return new RebelCmsPageResult();
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

        
    }
}
