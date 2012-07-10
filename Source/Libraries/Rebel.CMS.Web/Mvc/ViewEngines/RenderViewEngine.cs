using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Rebel.Cms.Web.Configuration;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model;
using Rebel.Framework;
using Rebel.Framework.Context;

namespace Rebel.Cms.Web.Mvc.ViewEngines
{
    /// <summary>
    /// A view engine to look into the template location specified in the config for the front-end/Rendering part of the cms,
    /// this includes paths to render partial macros and media item templates.
    /// </summary>
    public class RenderViewEngine : RazorViewEngine
    {
        
        private readonly RebelSettings _settings;
        
        private readonly IEnumerable<string> _supplementedViewLocations = new[] { "/{0}.cshtml", "/MediaTemplates/{0}.cshtml" };
        private readonly IEnumerable<string> _supplementedPartialViewLocations = new[] { "/{0}.cshtml", "/Partial/{0}.cshtml", "/Partials/{0}.cshtml", "/MacroPartials/{0}.cshtml" };

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="renderModelFactory"></param>
        public RenderViewEngine(RebelSettings settings, IRenderModelFactory renderModelFactory)
        {
            // TODO: Resolve TwoLevelViewCache problems in release builds, as it seems to cache views without taking parent folder into account
            ViewLocationCache = DefaultViewLocationCache.Null;
            //if (HttpContext.Current == null || HttpContext.Current.IsDebuggingEnabled)
            //{
            //    ViewLocationCache = DefaultViewLocationCache.Null;
            //}
            //else
            //{
            //    //override the view location cache with our 2 level view location cache
            //    ViewLocationCache = new TwoLevelViewCache(ViewLocationCache);
            //}

            _settings = settings;

            var replaceWithRebelFolder = _supplementedViewLocations.ForEach(location => _settings.RebelFolders.TemplateFolder + location);
            var replacePartialWithRebelFolder = _supplementedPartialViewLocations.ForEach(location => _settings.RebelFolders.TemplateFolder + location);

            //The Render view engine doesn't support Area's so make those blank
            ViewLocationFormats = replaceWithRebelFolder.ToArray();
            PartialViewLocationFormats = replacePartialWithRebelFolder.ToArray();

            AreaPartialViewLocationFormats = new string[] {};
            AreaViewLocationFormats = new string[] {};
        }

        public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            if (!ShouldFindView(controllerContext, false))
            {
                return new ViewEngineResult(new string[] { });
            }

            return base.FindView(controllerContext, viewName, masterName, useCache);
        }

        public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            if (!ShouldFindView(controllerContext, true))
            {
                return new ViewEngineResult(new string[] { });
            }

            return base.FindPartialView(controllerContext, partialViewName, useCache);
        }

        /// <summary>
        /// Determines if the view should be found, this is used for view lookup performance and also to ensure 
        /// less overlap with other user's view engines. This will return true if the Rebel back office is rendering
        /// and its a partial view or if the rebel front-end is rendering but nothing else.
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="isPartial"></param>
        /// <returns></returns>
        private bool ShouldFindView(ControllerContext controllerContext, bool isPartial)
        {
            //first check if we're rendering a partial view for the back office, or surface controller, etc...
            //anything that is not IRebelRenderModel as this should only pertain to Rebel views.
            if (isPartial
                && controllerContext.RouteData.DataTokens.ContainsKey("rebel")
                && !(controllerContext.RouteData.DataTokens["rebel"] is IRebelRenderModel))
            {
                return true;
            }

            //only find views if we're rendering the rebel front end
            if (controllerContext.RouteData.DataTokens.ContainsKey("rebel")
                && controllerContext.RouteData.DataTokens["rebel"] != null
                && controllerContext.RouteData.DataTokens["rebel"] is IRebelRenderModel)
            {
                return true;
            }

            return false;
        }

    }
}
