using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Macros;

namespace Rebel.Cms.Web.Mvc.ViewEngines
{
    public class RebelHelperViewPageActivator : IPostViewPageActivator
    {
        private readonly IRenderModelFactory _modelFactory;

        public RebelHelperViewPageActivator(IRenderModelFactory modelFactory)
        {
            _modelFactory = modelFactory;
        }

        public void OnViewCreated(ControllerContext context, Type type, object view)
        {
            if (!(view is IRequiresRebelHelper)) return;
            
            var typedView = view as IRequiresRebelHelper;

            //check if the view is already IRequiresRoutableRequestContext and see if its set, if so use it, otherwise
            //check if the current controller is IRequiresRoutableRequest context as it will be a bit quicker
            //to get it from there than to use the resolver, otherwise use the resolver.
            var routableRequestContext = this.GetRoutableRequestContextFromSources(view, context);           
            typedView.Rebel = new RebelHelper(context, routableRequestContext, _modelFactory);

        }

    }

 
}
