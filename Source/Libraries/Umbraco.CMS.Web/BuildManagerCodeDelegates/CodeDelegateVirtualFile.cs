using System;
using System.IO;
using System.Text;
using System.Web.Hosting;

namespace Umbraco.Cms.Web.BuildManagerCodeDelegates
{
    /// <summary>
    /// Represents a Virtual file for a code delegate
    /// </summary>
    internal class CodeDelegateVirtualFile : VirtualFile
    {

        internal CodeDelegateVirtualFile(string id, object delegateParameter, string virtualPath)
            : base(virtualPath)
        {
            _id = id;
            _delegateParameter = delegateParameter;
        }

        private readonly string _id;
        private readonly object _delegateParameter;

        /// <summary>
        /// Returns the id of the path
        /// </summary>
        public override string Name
        {
            get
            {
                return CodeDelegateVirtualPath.GetIdFromVirtualPath(VirtualPath);
            }
        }

        /// <summary>
        /// Opens the code delegate as as Stream
        /// </summary>
        /// <returns></returns>
        public override Stream Open()
        {
            Func<object, string> snippet;
            var method = CodeDelegatesCollection.TryGetDelegateMethod(_id);
            if (method.Success)
            {
                return new MemoryStream(Encoding.UTF8.GetBytes(method.Result(_delegateParameter, _id)));
            }
            return new MemoryStream(Encoding.UTF8.GetBytes(""));
        }

    }
}