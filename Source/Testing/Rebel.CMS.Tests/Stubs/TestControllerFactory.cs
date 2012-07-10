using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using Rebel.Cms.Web.System;
using Rebel.Framework;


namespace Rebel.Tests.Cms.Stubs
{
    /// <summary>
    /// Used in place of the RebelControllerFactory which relies on BuildManager which throws exceptions in a unit test context
    /// </summary>
    internal class TestControllerFactory : IControllerFactory
    {

        public IController CreateController(RequestContext requestContext, string controllerName)
        {
            var tf = new TypeFinder();
            var types = tf.FindClassesOfType<ControllerBase>(new[] { Assembly.GetExecutingAssembly() });

            var controllerTypes = types.Where(x => x.Name.Equals(controllerName + "Controller", StringComparison.InvariantCultureIgnoreCase));
            var t = controllerTypes.SingleOrDefault();
            
            if (t == null)
                return null;

            return Activator.CreateInstance(t) as IController;            
        }

        public System.Web.SessionState.SessionStateBehavior GetControllerSessionBehavior(RequestContext requestContext, string controllerName)
        {
            throw new NotImplementedException();
        }

        public void ReleaseController(IController controller)
        {
            throw new NotImplementedException();
        }
    }
}
