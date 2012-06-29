using System;
using System.Collections.Concurrent;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.BuildManagerCodeDelegates
{

    /// <summary>
    /// Allows for registration of Code snippet delegates for the CodeDelegateVirtualPathProvider to find
    /// </summary>
    internal static class CodeDelegatesCollection
    {
        
        /// <summary>
        /// Dictionary containing delegates used to return a code snippet
        /// </summary>
        /// <remarks>
        /// The key is the unique identifier of the code snippet (which must be created using the Create method of CodeDelegateVirtualPath),
        /// The value is the delegate which passes in the 'object key' to the delegate in order to get the code snippet returned.
        /// </remarks>
        private static readonly ConcurrentDictionary<string, Func<object, string, string>> SnippetCollection = new ConcurrentDictionary<string, Func<object, string, string>>();

        internal static void Clear()
        {
            SnippetCollection.Clear();
        }

        public static bool TryAdd(string virtualPathId, Func<object, string, string> codeDelegate)
        {
            if (!CodeDelegateVirtualPath.IsVirtualPathId(virtualPathId))
                throw new FormatException("The virtualPathId passed in must be created by the GetOrCreateVirtualPathId method of the class " + typeof(CodeDelegateVirtualPath).FullName);

            return SnippetCollection.TryAdd(virtualPathId, codeDelegate);
        }

        public static AttemptTuple<Func<object, string, string>> TryGetDelegateMethod(string virtualPathId)
        {
            Func<object, string, string> val;
            return SnippetCollection.TryGetValue(virtualPathId, out val) 
                ? new AttemptTuple<Func<object, string, string>>(true, val) 
                : AttemptTuple<Func<object, string, string>>.False;
        }
    }
}