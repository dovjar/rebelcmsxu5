using System;
using System.Web.Mvc;
using Rebel.Cms.Web.Context;

namespace Rebel.Cms.Web.Mvc.ViewEngines
{
    /// <summary>
    /// Checks if the controller that created it has the same context dependency and injects it
    /// </summary>
    [PostViewPageActivator(0)]
    public class RebelContextViewPageActivator : IPostViewPageActivator
    {
        public void OnViewCreated(ControllerContext context, Type type, object view)
        {
            if (view is IRequiresBackOfficeRequestContext && context.Controller is IRequiresBackOfficeRequestContext)
            {
                ((IRequiresBackOfficeRequestContext)view).BackOfficeRequestContext =
                    ((IRequiresBackOfficeRequestContext)context.Controller).BackOfficeRequestContext;
            }
            else if (view is IRequiresRoutableRequestContext && context.Controller is IRequiresRoutableRequestContext)
            {
                ((IRequiresRoutableRequestContext)view).RoutableRequestContext =
                    ((IRequiresRoutableRequestContext)context.Controller).RoutableRequestContext;
            }
        }
    }
}