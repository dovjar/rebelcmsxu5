using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Mapping;
using Rebel.Cms.Web.Model;
using Rebel.Cms.Web.Model.BackOffice.Editors;
using Rebel.Framework;
using Rebel.Framework.Caching;
using Rebel.Framework.Persistence.ModelFirst;
using Rebel.Hive;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.RepositoryTypes;
using File = Rebel.Framework.Persistence.Model.IO.File;

namespace Rebel.Cms.Web.Caching
{
    public class ContentCacheManager
    {
        private const string JsonKey = "format=json";
        private const int LongTime = 999;

        private readonly IRebelApplicationContext _context;
        private readonly UrlHelper _urlHelper;
        private readonly ControllerContext _controllerContext;
        private readonly ViewDataDictionary _viewData;
        private readonly TempDataDictionary _tempData;
        private readonly HttpRequestBase _requestContext;
        private static readonly object Mutex = new Object();

        public ContentCacheManager(IRebelApplicationContext context, Controller controller)
        {
            _context = context;
            _urlHelper = controller.Url;
            _controllerContext = controller.ControllerContext;
            _viewData = controller.ViewData;
            _tempData = controller.TempData;
            _requestContext = controller.HttpContext.Request;
            
            EnsureCacheWarmed();
        }

        private void EnsureCacheWarmed()
        {
            var host = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);

            if (_context.IsFirstRun) // lets warm up the cache if this is the first request
            {
                lock (Mutex)
                {
                    if (!_context.IsFirstRun) return;
                    _context.IsFirstRun = false;

                    Task.Factory.StartNew(() =>
                                              {
                                                  var cacheWarmer = new CacheWarmer(host);
                                                  cacheWarmer.TraverseFrom("/");
                                              });
                }
            }
        }

        private bool IsJsonRequest()
        {
            return _requestContext.RawUrl.Contains(JsonKey) || _requestContext.ContentType == "application/json";
        }

        public KeyValuePair<string, string> RenderContent(IRebelRenderModel model)
        {
            bool isPreview;
            Boolean.TryParse(_requestContext.QueryString[ContentEditorModel.PreviewQuerystringKey], out isPreview);

            if (IsJsonRequest())
            {
                var jsonResult = isPreview
                                     ? RenderAsJson(model)
                                     : Cache(model.CurrentNode.NiceUrl() + "/json", () => RenderAsJson(model));

                return new KeyValuePair<string, string>("application/json",jsonResult);
            }

            var htmlResult = GetViewHtml(model);

            if (string.IsNullOrEmpty(htmlResult))
                return new KeyValuePair<string, string>(string.Empty, string.Empty);

            return new KeyValuePair<string, string>("text/html", htmlResult);
        }

        private string Cache(string key, Func<string> value)
        {
            using (DisposableTimer.TraceDuration<DefaultRenderModelFactory>("Begin Parsing HTML", "End Parsing HTML"))
            {
                return _context
                        .FrameworkContext.Caches.ExtendedLifetime.GetOrCreate(
                        key, value, new StaticCachePolicy(TimeSpan.FromDays(LongTime))).Value.Item;
            }
        }

        private string GetViewHtml(IRebelRenderModel model)
        {
            using (IReadonlyGroupUnit<IFileStore> uow =
                _context.Hive.OpenReader<IFileStore>(new Uri("storage://templates")))
            {
                File templateFile = model.CurrentNode.CurrentTemplate != null
                                        ? uow.Repositories.Get<File>(model.CurrentNode.CurrentTemplate.Id)
                                        : null;

                if (templateFile != null)
                {
                    return RenderRazorViewToString(templateFile, model.CurrentNode);
                }
            }

            return String.Empty;
        }

        private string RenderAsJson(IRebelRenderModel model)
        {
            var typedEntity = (CustomTypedEntity<Content>)model.CurrentNode;
            var mapper = new SimpleFlattenedTypedEntityMapper(
                    _context.Hive, _urlHelper);

            var flattenedObject = mapper.Flatten(model.CurrentNode.NiceUrl(), typedEntity);
            return flattenedObject.ToJsonString();
        }

        private string RenderRazorViewToString(File templateFile, object model)
        {
            _viewData.Model = model;
            ViewEngineResult view = ViewEngines.Engines.FindView(_controllerContext, 
                Path.GetFileNameWithoutExtension(templateFile.RootedPath), "");

            using (var sw = new StringWriter())
            {
                var viewContext = new ViewContext(_controllerContext, view.View, _viewData, _tempData, sw);
                view.View.Render(viewContext, sw);
                view.ViewEngine.ReleaseView(_controllerContext, view.View);
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}
