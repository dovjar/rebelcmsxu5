using System;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Mvc.Controllers;
using Rebel.Cms.Web.Mvc.Controllers.BackOffice;
using Rebel.Cms.Web.Mvc.ViewEngines;
using Rebel.Framework;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Cms.Web.Editors.Extenders
{
    /// <summary>
    /// Base class for Content Controller Extenders
    /// </summary>    
    public class ContentControllerExtenderBase : SecuredBackOfficeController
    {
        public ContentControllerExtenderBase(IBackOfficeRequestContext backOfficeRequestContext)
            : base(backOfficeRequestContext)
        {
           
        }

        protected dynamic ContentController;

        /// <summary>
        /// This checks for the parent controller type and validates it, then sets the appropriate properties
        /// </summary>
        /// <param name="requestContext"></param>
        protected override void Initialize(global::System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);

            var parentContext = RouteData.GetControllerExtenderParentContext();
            if (parentContext == null)
                throw new NotSupportedException(this.GetType().Name + " cannot be instantiated unless it is created as a Controller Extender");
            var baseType = parentContext.Controller.GetType().BaseType;
            if (baseType == null || !baseType.Name.StartsWith(typeof(AbstractRevisionalContentEditorController<>).Name))
                throw new NotSupportedException(this.GetType().Name + " cannot be instantiated unless it is created as a Controller Extender for type " + typeof(AbstractRevisionalContentEditorController<>).Name);
            ContentController = (dynamic)parentContext.Controller;
            
            //now that we have a reference to the parent content controller, assign the ViewBag values which are normally required for creating forms in views            
            ViewBag.ControllerId = RebelController.GetControllerId<EditorAttribute>(ContentController.GetType());
         
        }

        private GroupUnitFactory _hive;
        protected virtual GroupUnitFactory Hive
        {
            get 
            { 
                if (_hive == null)
                {
                    _hive = BackOfficeRequestContext.Application.Hive.GetWriter<IContentStore>();
                    Mandate.That(_hive != null, x => new NullReferenceException("Could not find hive provider for route content://"));
                }
                return _hive; 
            }
        }
    }
}