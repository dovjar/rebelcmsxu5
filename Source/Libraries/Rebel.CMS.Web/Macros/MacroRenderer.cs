using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Caching;
using System.Web.Mvc;
using System.Globalization;

namespace Rebel.Cms.Web.Macros
{
    using Rebel.Cms.Web.Context;
    using Rebel.Cms.Web.DependencyManagement;
    using Rebel.Cms.Web.Model;
    using Rebel.Cms.Web.Model.BackOffice.Editors;
    using Rebel.Framework;
    using Rebel.Framework.Context;
    using Rebel.Framework.Diagnostics;
    using Rebel.Framework.Localization;
    using Rebel.Framework.Persistence.Model.IO;
    using Rebel.Hive;
    using Rebel.Hive.RepositoryTypes;
    using Rebel.Framework.Security.Model.Entities;

    /// <summary>
    /// A utility class for rendering macros either as ActionResult or as string
    /// </summary>
    public class MacroRenderer
    {
        public const string MacroPageParameterKey = "INTERNAL-MACRO-PAGEID";
        public const string MacroMemberParameterKey = "INTERNAL-MACRO-MEMBERID";
        public const string MacroCachePrefix = "umb-macro-";

        private readonly ComponentRegistrations _componentRegistrations;
        private readonly IRoutableRequestContext _routableRequestContext;
        private readonly IRebelApplicationContext _applicationContext;

        /// <summary>
        /// Creates a new instance of the MacroRender class that provides utility methods for compiling and rendering macros.
        /// </summary>
        public MacroRenderer(ComponentRegistrations componentRegistrations, IRoutableRequestContext routableRequestContext)
        {
            _componentRegistrations = componentRegistrations;
            _routableRequestContext = routableRequestContext;
            _applicationContext = routableRequestContext.Application;
        }

        /// <summary>
        /// Renders/executes the provided macro ActionResult.
        /// </summary>
        /// <remarks>Macro rendering time is traced.</remarks>
        private static string GetMacroStringOutput(string macroAlias, ActionResult ar, ControllerContext currentControllerContext)
        {
            using (DisposableTimer.Start(timer =>
                            LogHelper.TraceIfEnabled<MacroRenderer>("GetMacroStringOutput for {0} took {1}ms", () => macroAlias, () => timer)))
            {
                if (ar is ViewResultBase)
                {
                    var viewResult = (ViewResultBase)ar;

                    return currentControllerContext.RenderViewResultAsString(viewResult);
                }
                if (ar is ContentResult)
                {
                    var contentResult = (ContentResult)ar;
                    return contentResult.Content;
                }
                throw new NotSupportedException("Its not possible to retreive the output of a macro that doesn't return a ViewResultBase");
            }
        }

        /// <summary>
        /// Renders the macro as string.
        /// </summary>
        /// <param name="macroAlias">The macro alias.</param>
        /// <param name="macroParams">The macro params.</param>
        /// <param name="currentControllerContext">The current controller context.</param>
        /// <param name="isForRichTextEditor">if set to <c>true</c> [is for rich text editor].</param>
        /// <param name="resolveContent">The method to call in order to resolve the content to render.</param>
        /// <returns></returns>
        public string RenderMacroAsString(string macroAlias, IDictionary<string, string> macroParams, ControllerContext currentControllerContext, bool isForRichTextEditor, Func<Content> resolveContent)
        {
            return RenderMacroAsString(macroAlias, macroParams, currentControllerContext, isForRichTextEditor, resolveContent, null);
        }

        /// <summary>
        /// Renders the macro output as a string
        /// </summary>
        /// <param name="macroAlias">The macro alias.</param>
        /// <param name="macroParams">The macro params.</param>
        /// <param name="currentControllerContext">The current controller context.</param>
        /// <param name="isForRichTextEditor">If the request is to render the contents in the back office rich text editor</param>
        /// <param name="resolveContent">The method to call in order to resolve the content to render.</param>
        /// <param name="resolveMember">The method to call in order to resolve the current member. If null, this parameter is ignored</param>
        /// <returns></returns>
        public string RenderMacroAsString(
            string macroAlias,
            IDictionary<string, string> macroParams,
            ControllerContext currentControllerContext,
            bool isForRichTextEditor,
            Func<Content> resolveContent, /* CurrentPage */
            Func<Member> resolveMember /* CurrentMember */)
        {

            // if we're rendering inside the rich text editor, bypass all caching and return the fresh result.
            if (isForRichTextEditor)
            {
                return GetMacroStringOutput(
                    macroAlias,
                    RenderMacro(macroAlias, macroParams, currentControllerContext, true, resolveContent),
                    currentControllerContext);
            }

            // Macro text is cached in the ApplicationCache based on it's parameters, se we need to build that cache key...

            // get the macro model
            var macroModel = GetMacroModel(macroAlias);

            // get the content id to pass in to the macro cache key builder.
            // null indicates not to cache by page.
            HiveId? contentId = null;
            if (macroModel.Item1.CacheByPage)
            {
                if (resolveContent != null)
                    contentId = resolveContent().Id;
            }

            // get the member id to pass into the macro cache key builder.
            // null indicates not to cache by member.
            HiveId? memberId = null;
            if (macroModel.Item1.CachePersonalized)
            {
                if (resolveMember != null)
                    memberId = resolveMember().Id;
            }

            // build the cache key of the macro
            var macroCacheKey = GetMacroCacheKey(macroAlias, macroParams, contentId, memberId);

            // now generate the content for it.
            string macroContent = null;
            string cachedMacroContent = null;

            cachedMacroContent = _applicationContext.FrameworkContext.ApplicationCache.GetOrCreate(macroCacheKey, () =>
            {
                // generate the content of the macro, since it's not in the cache.
                macroContent = GetMacroStringOutput(
                    macroAlias,
                    RenderMacro(macroAlias, macroParams, currentControllerContext, false, resolveContent),
                    currentControllerContext);

                // bail if cache period is zero.
                if (macroModel.Item1.CachePeriodSeconds <= 0) return null;

                // return the cache parameters
                var cacheParams = new HttpRuntimeCacheParameters<string>(macroContent);
                if (macroModel.Item2 != null && macroModel.Item2 is File)
                {
                    // add file dependency
                    cacheParams.Dependencies = new CacheDependency(macroModel.Item2.RootedPath);
                }
                cacheParams.SlidingExpiration = new TimeSpan(0, 0, 0, macroModel.Item1.CachePeriodSeconds);
                return cacheParams;
            });

            // return either the cahed contnet or the generated content.
            return cachedMacroContent ?? macroContent;
        }
                
        private Tuple<MacroEditorModel, File> GetMacroModel(string macroAlias)
        {
            // TODO: Implement caching here?  
         
            using (var uow = _applicationContext.Hive.OpenReader<IFileStore>(new Uri("storage://macros")))
            {
                var filename = macroAlias + ".macro";
                var macroFile = uow.Repositories.Get<File>(new HiveId(filename));
                if (macroFile == null)
                    throw new ApplicationException("Could not find a macro with the specified alias: " + macroAlias);
                return new Tuple<MacroEditorModel, File>(MacroSerializer.FromXml(Encoding.UTF8.GetString(macroFile.ContentBytes)), macroFile);
            }
        }

        private ActionResult GetMacroResult(string macroAlias, Func<MacroEditorModel> getMacroMethod, Func<Content> getNodeMethod, IDictionary<string, string> macroParams, ControllerContext currentControllerContext)
        {
            var currentNode = getNodeMethod();
            if (currentNode == null)
            {
                //If we have no current node (i.e. its new content rendering in the TinyMCE editor), then
                //we can only return a friendly content message.
                return null;
            }
            var macro = getMacroMethod();

            var engine = _componentRegistrations.MacroEngines.SingleOrDefault(x => x.Metadata.EngineName.InvariantEquals(macro.MacroType));
            if (engine == null)
            {
                throw new InvalidOperationException("Could not find a MacroEngine with the name " + macro.MacroType);
            }

            try
            {
                //execute the macro engine
                return engine.Value.Execute(
                    currentNode,
                    macroParams,
                    new MacroDefinition { MacroEngineName = macro.MacroType, SelectedItem = macro.SelectedItem },
                    currentControllerContext,
                    _routableRequestContext);
            }
            catch (Exception ex)
            {
                //if there's an exception, display a friendly message and log the error
                var txt = "Macro.RenderingFailed.Message".Localize(this, new { Error = ex.Message, MacroName = macroAlias });
                var title = "Macro.RenderingFailed.Title".Localize();
                LogHelper.Error<MacroRenderer>(txt, ex);
                return MacroError(txt, title);
            }
        }

        /// <summary>
        /// Returns an ActionResult for the specified Macro.
        /// </summary>
        /// <param name="macroAlias"></param>
        /// <param name="currentControllerContext"></param>
        /// <param name="isForRichTextEditor">If the request is to render the contents in the back office rich text editor</param>
        /// <param name="resolveContent">callback to get the 'Content' model</param>
        public ActionResult RenderMacro(
            string macroAlias,
            IDictionary<string, string> macroParams,
            ControllerContext currentControllerContext,
            bool isForRichTextEditor,
            Func<Content> resolveContent)
        {            
            using (DisposableTimer.Start(timer =>
                            LogHelper.TraceIfEnabled<MacroRenderer>("RenderMacro for {0} took {1}ms", () => macroAlias, () => timer)))
            {
                try
                {
                    // get the macro's model
                    var macroModel = GetMacroModel(macroAlias);

                    if (isForRichTextEditor && !macroModel.Item1.RenderContentInEditor)
                    {
                        return NoRichTextRenderMode(macroAlias);
                    }
                    else
                    {
                        return GetMacroResult(macroAlias, () => macroModel.Item1, resolveContent, macroParams, currentControllerContext);
                    }
                }
                catch (ApplicationException ex)
                {
                    //if there's an exception, display a friendly message and log the error
                    var txt = "Macro.RenderingFailed.Message".Localize(this, new { Error = ex.Message, MacroName = macroAlias });
                    var title = "Macro.RenderingFailed.Title".Localize();
                    LogHelper.Error<MacroRenderer>(txt, ex);
                    return MacroError(txt, title);
                }
            }
        }

        public static int ClearCacheByPrefix(IFrameworkContext context)
        {
            return context.ApplicationCache.InvalidateItems(MacroCachePrefix + "*");
        }

        /// <summary>
        /// Creates a cache key for the macro's parameters and cache options.
        /// </summary>
        /// <param name="macroParams">A dicitonary of the parameters</param>
        /// <param name="currentPageId">The current page hiveId.  If null, CacheByPage is false.</param>
        /// <param name="currentMemberId">The current member hiveId.  If null, CachePersonalized is false.</param>
        /// <returns>A repeatable string key that represents the macro settings</returns>
        private static string GetMacroCacheKey(string macroAlias, IDictionary<string, string> macroParams, HiveId? currentPageId, HiveId? currentMemberId)
        {
            // hold the string while we build it up.
            var keyBuilder = new StringBuilder();

            keyBuilder.Append(MacroCachePrefix);
            keyBuilder.Append(macroAlias.ToUpperInvariant());
            keyBuilder.Append("-");            

            // cache by page and member are added to the key FIRST
            // so that it is easier to debug/view.

            // cache by page?
            if (currentPageId.HasValue && currentPageId.Value != HiveId.Empty)
            {
                var cacheByPageId = currentPageId.Value;

                // if the macroParams contains this item, that means the user is
                // overriding the requested item for the specified page.
                var userCurrentPageOverride = macroParams.GetEntryIgnoreCase(MacroPageParameterKey);
                if (!userCurrentPageOverride.IsNullOrWhiteSpace())
                {
                    var parsed = HiveId.TryParse(userCurrentPageOverride);
                    if (parsed.Success && parsed.Result != HiveId.Empty)
                    {
                        cacheByPageId = parsed.Result;
                    }
                }

                // add it to the key
                AppendMacroParam(keyBuilder, MacroPageParameterKey, cacheByPageId);
            }
            
            // personalized by member?
            if (currentMemberId.HasValue)
            {
                var cacheByMemberId = currentMemberId.Value;

                // if macroParams contains this item, we override it with the developer supplied value.
                var developerMemberOverride = macroParams.GetEntryIgnoreCase(MacroMemberParameterKey);
                if (!developerMemberOverride.IsNullOrWhiteSpace())
                {
                    var parsed = HiveId.TryParse(developerMemberOverride);
                    if (parsed.Success)
                    {
                        cacheByMemberId = parsed.Result;
                    }
                }

                // add it to the key
                AppendMacroParam(keyBuilder, MacroMemberParameterKey, cacheByMemberId);
            }
            
            // now add all of the macro parameters to the key!
            macroParams.OrderBy(kvp => kvp.Key).ForEach(kvp =>
                AppendMacroParam(keyBuilder, kvp.Key, kvp.Value));

            // return the stringed key!
            return keyBuilder.ToString();
        }

        /// <summary>
        /// Used by GetMacroCacheKey to consistently add a key:value pair to the param key generator.
        /// </summary>
        private static void AppendMacroParam(StringBuilder paramKeyGenerator, string key, object value)
        {
            var paramKey = key;
            if (paramKey == string.Empty)
                paramKey = "(empty key)";
            else if (paramKey == null)
                paramKey = "(null key)";

            var paramValue = value;
            if (paramValue == null)
                paramValue = "(null value)";

            paramKeyGenerator.AppendFormat(CultureInfo.InvariantCulture, "[{0}]:{1};",
                paramKey.ToUpperInvariant(),
                paramValue);
        }
        
        /// <summary>
        /// Returns the html to display an error inline
        /// </summary>
        /// <param name="error"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public ContentResult MacroError(string error, string title)
        {
            return new ContentResult
            {
                Content = "<div style='color:#8a1f11;padding:4px;border:2px solid #FBC2C4;'><h4 style='color:#8a1f11;font-weight:bold;margin:0;font-size:14px;'>" + title + "</h4><p style='margin:0;font-size:12px;'>" + error + "</p></div>"
            };
        }

        /// <summary>
        /// Returns the content to display in the Rich Text Editor when the macro is flagged to not render contents
        /// </summary>
        /// <returns></returns>
        public ContentResult NoRichTextRenderMode(string macroAlias)
        {
            return new ContentResult
            {
                Content = "<div style='color:#514721;'>Macro: '<strong>" + macroAlias + "</strong>'</div>"
            };
        }
    }
}