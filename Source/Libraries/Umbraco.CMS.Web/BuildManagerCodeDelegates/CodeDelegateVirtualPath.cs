using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.BuildManagerCodeDelegates
{

    /// <summary>
    /// Represents the delegate parameter that will be passed into the delegate to return the code block
    /// </summary>
    internal class CodeDelegateParameter
    {

        public CodeDelegateParameter(object p)
        {
            UtcTicks = DateTime.UtcNow.Ticks;
            Parameter = p;
        }
        
        /// <summary>
        /// The object that will be passed into the delegate as a parameter
        /// </summary>
        public object Parameter { get; private set; }

        /// <summary>
        /// The UTC Ticks of when this object is created
        /// </summary>
        public long UtcTicks { get; private set; }
    }

    /// <summary>
    /// A static helper class for creating new Ids for use in storing code snippets in the CodeDelegatesCollection collection
    /// </summary>
    internal static class CodeDelegateVirtualPath
    {
        internal const string PrefixId = "DVP";
        internal const string PathPrefix = "/" + PrefixId + ".axd/";
        internal const string NamePrefix = "_" + PrefixId + "_";
        private static readonly Regex RegexPathPrefix = new Regex(PathPrefix, RegexOptions.Compiled);
        private static readonly Regex RegexNamePrefix = new Regex(NamePrefix, RegexOptions.Compiled);

        /// <summary>
        /// Internal collection of path ids and it's definition
        /// </summary>
        private static readonly ConcurrentDictionary<string, CodeDelegateParameter> VirtualPathIds = new ConcurrentDictionary<string, CodeDelegateParameter>();

        internal static void ClearPathIds()
        {
            VirtualPathIds.Clear();
        }

        /// <summary>
        /// Returns the virtual path id and the CodeDelegateParameter for the virtualPathId if one exists
        /// </summary>
        /// <param name="virtualPathId"></param>
        /// <returns></returns>
        public static AttemptTuple<KeyValuePair<string, CodeDelegateParameter>> TryGetVirtualPathDefinition(string virtualPathId)
        {
            //first check if the parameter exists in the values already and return the virtual path id
            if (VirtualPathIds.ContainsKey(virtualPathId) )
            {
                return new AttemptTuple<KeyValuePair<string, CodeDelegateParameter>>(
                    true,
                    new KeyValuePair<string, CodeDelegateParameter>(virtualPathId, VirtualPathIds[virtualPathId]));
            }
            else 
            {
                return AttemptTuple<KeyValuePair<string, CodeDelegateParameter>>.False;
            }
                
        }

        /// <summary>
        /// returns all virtual path ids that have been registered with the delegate parameter
        /// </summary>
        /// <param name="delegateParameter"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetVirtualPathIdsForDelegate(object delegateParameter)
        {
            return VirtualPathIds.Where(x => x.Value.Parameter.Equals(delegateParameter)).Select(x => x.Key);
        }

        /// <summary>
        /// Attempts to update a delegateParameter for the virtualPathId specified if one exists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="virtualPathId"></param>
        /// <param name="delegateParameter"></param>
        /// <returns></returns>
        public static bool TryUpdate<T>(string virtualPathId, T delegateParameter)
        {
            CodeDelegateParameter existingParam;
            if (VirtualPathIds.TryGetValue(virtualPathId, out existingParam))
            {
                return VirtualPathIds.TryUpdate(virtualPathId, new CodeDelegateParameter(delegateParameter), existingParam);
            }
            return false;
        }

        /// <summary>
        /// Creates a new Id for the code delegate, if one is already found then we will return the id that already exists.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="delegateParameter">
        /// An object representing the delegate, this object will be passed in to the delegate as a parameter which
        /// can be used in the delegate method body to return the code snippet
        /// </param>
        /// <param name="virtualPath">
        /// The unique Id of the object which will be used to create the virtual path, this id will be passed into the 
        /// delegate used to return the code snippet, in most cases, this id will contain information that can be used
        /// in the delegate for data retreival.
        /// 
        /// If not supplied, then the ToString method of the delegateParameter will be used.
        /// </param>
        /// <returns></returns>
        public static string GetOrCreateVirtualPathId<T>(T delegateParameter, string virtualPath = null)
        {
            if (virtualPath == null)
            {
                virtualPath = delegateParameter.ToString();
            }

            //we need to add a unique number to the end because if we want to invalidate the dynamically generated class, then
            //the path will need to be different. GetHashCode of DateTime will be enough, it simply folds over the ticks. We don't
            //want to use ticks cuz that will make the  path longer.
            var id = NamePrefix + virtualPath; // +Math.Abs(DateTime.UtcNow.GetHashCode());

            //first check if it already exists
            var attempt = TryGetVirtualPathDefinition(id);
            if (attempt.Success)
                return attempt.Result.Key;

            if (!Regex.IsMatch(virtualPath, @"^\w+$"))
                throw new InvalidOperationException("The virtualPathId must be alphanumeric");

            if (!VirtualPathIds.ContainsKey(id))
            {
                //add the mapping
                VirtualPathIds.TryAdd(id, new CodeDelegateParameter(delegateParameter));
            }
            return id;
        }

        /// <summary>
        /// Determines if the path passed in is a code delegate id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal static bool IsVirtualPathId(string id)
        {
            Mandate.ParameterNotNullOrEmpty(id, "id");

            if (RegexNamePrefix.IsMatch(id))
            {
                if (!string.IsNullOrEmpty(id))
                    return VirtualPathIds.ContainsKey(id);
                return false;
            }

            return false;
        }

        /// <summary>
        /// Determines if the path passed in is a code delegate path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static bool IsCodeDelegateVirtualPath(string path)
        {
            Mandate.ParameterNotNullOrEmpty(path, "path");

            if (RegexPathPrefix.IsMatch(path))
            {
                var id = GetIdFromVirtualPath(path);
                return IsVirtualPathId(id);
            }

            return false;
        }

        /// <summary>
        /// Extracts the code delegate id from the virtual path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static string GetIdFromVirtualPath(string path)
        {
            Mandate.ParameterNotNullOrEmpty(path, "path");

            var match = Regex.Match(path, @"/" + PrefixId + @"\.axd.*/(" + NamePrefix + @"\w+)\.\w+", RegexOptions.Compiled);
            if (match.Success && match.Groups.Count == 2)
                return match.Groups[1].Value;

            return string.Empty;
        }

        /// <summary>
        /// Returns a full virtual path for the code delegate id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        internal static string CreateFullPath(string id, string extension)
        {
            Mandate.That<FormatException>(!extension.StartsWith("."));
            if (!id.StartsWith(NamePrefix))
            {
                throw new NotSupportedException("The id referenced is an invalid format");
            }
            return string.Format("~" + PathPrefix + "{0}.{1}", id, extension);
        }
    }
}