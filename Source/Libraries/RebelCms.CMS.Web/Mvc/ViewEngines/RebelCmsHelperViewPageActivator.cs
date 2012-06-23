using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Macros;

namespace RebelCms.Cms.Web.Mvc.ViewEngines
{
    public class RebelCmsHelperViewPageActivator : IPostViewPageActivator
    {
        private readonly IRenderModelFactory _modelFactory;

        public RebelCmsHelperViewPageActivator(IRenderModelFactory modelFactory)
        {
            _modelFactory = modelFactory;
        }

        public void OnViewCreated(ControllerContext context, Type type, object view)
        {
            if (!(view is IRequiresRebelCmsHelper)) return;
            
            var typedView = view as IRequiresRebelCmsHelper;

            //check if the view is already IRequiresRoutableRequestContext and see if its set, if so use it, otherwise
            //check if the current controller is IRequiresRoutableRequest context as it will be a bit quicker
            //to get it from there than to use the resolver, otherwise use the resolver.
            var routableRequestContext = this.GetRoutableRequestContextFromSources(view, context);           
            typedView.RebelCms = new RebelCmsHelper(context, routableRequestContext, _modelFactory);

        }

    }

 
}
