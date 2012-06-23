using RebelCms.Cms.Web.Routing;

namespace RebelCms.Cms.Web
{
    public static class UrlResolutionResultExtensions
    {
        /// <summary>
        /// If the URL Resolution was successful (didn't return an error status)
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool IsSuccess(this UrlResolutionResult result)
        {
            return (int)result.Status < 10;
        }
    }
}