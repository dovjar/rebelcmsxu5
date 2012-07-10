using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;
using System.Web.Hosting;
using Rebel.Framework;

namespace Rebel.Cms.Web.BuildManagerCodeDelegates
{

    /// <summary>
    /// A virtual path provider to find any code snippets that have been registered in the CodeDelegatesCollection collection
    /// </summary>
    internal class CodeDelegateVirtualPathProvider : VirtualPathProvider
    {

        public override bool FileExists(string virtualPath)
        {
            return CodeDelegateVirtualPath.IsCodeDelegateVirtualPath(virtualPath) || (Previous != null && Previous.FileExists(virtualPath));
        }

        /// <summary>
        /// Returns the view resource stream 
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <returns></returns>
        public override VirtualFile GetFile(string virtualPath)
        {
            if (CodeDelegateVirtualPath.IsCodeDelegateVirtualPath(virtualPath))
            {
                //to get the file we need to get the id
                var id = CodeDelegateVirtualPath.GetIdFromVirtualPath(virtualPath);
                var def = CodeDelegateVirtualPath.TryGetVirtualPathDefinition(id);
                if (def.Success)
                {
                    return new CodeDelegateVirtualFile(id, def.Result.Value.Parameter, virtualPath);
                }                
            }

            //let the base class handle this
            return Previous.GetFile(virtualPath);
        }

        //TODO: Test that invalidation recompiles a new assembly!

        /// <summary>
        /// Override the GetCacheKey to return a key for the virtual path that is unique to the timestamp that the code snippet
        /// was registered, this allows for invalidating a code snippet item which will force a re-compile
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <returns></returns>
        public override string GetCacheKey(string virtualPath)
        {
            if (CodeDelegateVirtualPath.IsCodeDelegateVirtualPath(virtualPath))
            {
                var virtualPathId = CodeDelegateVirtualPath.TryGetVirtualPathDefinition(
                    CodeDelegateVirtualPath.GetIdFromVirtualPath(virtualPath));
                if (virtualPathId.Success)
                {
                    //the id + the ticks will be unique
                    return virtualPathId.Result.Key + virtualPathId.Result.Value.UtcTicks;
                }                
            }
            return base.GetCacheKey(virtualPath);
        }

        /// <summary>
        /// We return null for the cache dependency since we cannot provide any absolute paths to files
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <param name="virtualPathDependencies"></param>
        /// <param name="utcStart"></param>
        /// <returns></returns>
        public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            if (CodeDelegateVirtualPath.IsCodeDelegateVirtualPath(virtualPath))
            {
                return null;
            }
            return Previous.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
        }
    }
}