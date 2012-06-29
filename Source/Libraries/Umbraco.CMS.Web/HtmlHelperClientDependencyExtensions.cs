using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using ClientDependency.Core.Mvc;
using Umbraco.Cms.Web.Context;

namespace Umbraco.Cms.Web
{
    public static class HtmlHelperClientDependencyExtensions
    {

        /// <summary>
        /// Puts a dependency on the tree css sprites
        /// </summary>
        /// <param name="html"></param>
        public static HtmlHelper RequiresTreeCssSprites(this HtmlHelper html)
        {
            var backOfficeRequestContext = DependencyResolver.Current.GetService<IBackOfficeRequestContext>();
            foreach (var path in backOfficeRequestContext.DocumentTypeIconResolver.Sprites
                .Select(sprites =>
                    backOfficeRequestContext.Application.Settings.UmbracoFolders.DocTypeIconFolder + "/" + sprites.Value.Name))
            {
                html.RequiresCss(path);
            }
            return html;
        }

        /// <summary>
        /// Puts a dependency on the application css sprites
        /// </summary>
        /// <param name="html"></param>
        public static HtmlHelper RequiresAppCssSprites(this HtmlHelper html)
        {
            var backOfficeRequestContext = DependencyResolver.Current.GetService<IBackOfficeRequestContext>();
            foreach (var path in backOfficeRequestContext.ApplicationIconResolver.Sprites
                .Select(sprites =>
                    backOfficeRequestContext.Application.Settings.UmbracoFolders.ApplicationIconFolder + "/" + sprites.Value.Name))
            {
                html.RequiresCss(path);
            }
            return html;
        }

        /// <summary>
        /// Puts a dependency on an entire folder
        /// </summary>
        /// <param name="html"></param>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public static HtmlHelper RequiresJsFolder(this HtmlHelper html, string folderPath)
        {
            return html.RequiresJsFolder(folderPath, 100);
        }

        /// <summary>
        /// Puts a dependency on an entire folder
        /// </summary>
        /// <param name="html"></param>
        /// <param name="folderPath"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        public static HtmlHelper RequiresJsFolder(this HtmlHelper html, string folderPath, int priority)
        {
            var httpContext = html.ViewContext.HttpContext;
            var systemRootPath = httpContext.Server.MapPath("~/");
            var folderMappedPath = httpContext.Server.MapPath(folderPath);

            if (folderMappedPath.StartsWith(systemRootPath))
            {
                var files = Directory.GetFiles(folderMappedPath, "*.js", SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    var absoluteFilePath = "~/" + file.Substring(systemRootPath.Length).Replace("\\", "/");
                    html.RequiresJs(absoluteFilePath, priority);
                }
            }

            return html;
        }
    }
}
