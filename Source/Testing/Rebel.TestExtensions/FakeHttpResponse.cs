using System.Web;

namespace Rebel.Tests.Extensions
{
    internal class FakeHttpResponse : HttpResponseBase
    {
        public override string ApplyAppPathModifier(string virtualPath)
        {
            return virtualPath;
        }
    }
}