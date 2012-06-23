using System.Web;

namespace RebelCms.Tests.Extensions
{
    internal class FakeHttpResponse : HttpResponseBase
    {
        public override string ApplyAppPathModifier(string virtualPath)
        {
            return virtualPath;
        }
    }
}