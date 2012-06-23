using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Mvc.Controllers;


namespace RebelCms.Cms.Web.Mvc.ActionInvokers
{
    /// <summary>
    /// Ensures that if an action for the Template name is not explicitly defined by a user, that the 'Index' action will execute
    /// </summary>
    public class RebelCmsActionInvoker : RoutableRequestActionInvoker
    {
        public RebelCmsActionInvoker(IRoutableRequestContext routableRequestContext)
            : base(routableRequestContext)
        { }

        /// <summary>
        /// Ensures that if an action for the Template name is not explicitly defined by a user, that the 'Index' action will execute
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="controllerDescriptor"></param>
        /// <param name="actionName"></param>
        /// <returns></returns>
        protected override ActionDescriptor FindAction(ControllerContext controllerContext, ControllerDescriptor controllerDescriptor, string actionName)
        {
            var ad = base.FindAction(controllerContext, controllerDescriptor, actionName);
            
            //now we need to check if it exists, if not we need to return the Index by default
            if (ad == null)
            {
                //check if the controller is an instance of IRebelCmsController
                if (controllerContext.Controller is RebelCmsController)
                {
                    //return the Index method info object of the IRebelCmsController
                    return new ReflectedActionDescriptor(((RebelCmsController)controllerContext.Controller).GetType().GetMethod("Index"), "Index", controllerDescriptor);
                }
            }
            return ad;
        }

    }
}