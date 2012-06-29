using System.Web.Mvc;

namespace Umbraco.Cms.Web
{
    /// <summary>
    /// The result from Proxying a request to another controller's action
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public class ProxyRequestResult<TResult>
        where TResult : ActionResult
    {
        public ProxyRequestResult(string renderedOutput, TResult result)
        {
            RenderedOutput = renderedOutput;
            Result = result;
        }

        /// <summary>
        /// The string rendered output from proxying a request to another controller's action
        /// </summary>
        public string RenderedOutput { get; private set; }

        /// <summary>
        /// The ActionResult result from proxying
        /// </summary>
        /// <remarks>
        /// IMPORTANT: this result should not be used to return from another Action since this has 
        /// already executed the full controller process including wiring up views and rendering them. If you return this ActionResult
        /// from another Action, it will not have it's views wired up, you would need to call the proxied controller's ControllerContext
        /// method EnsureViewObjectDataOnResult. This however will end up performing the same execution twice, since the controller has already
        /// been executed hence why this method returns a string (the already rendered result from the controller execution).
        /// 
        /// This ActionResult may contain other data in the form of properties which may be useful which is why it is being returned.
        /// </remarks>
        public TResult Result { get; private set; }
    }
}