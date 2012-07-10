using System.Web.Mvc;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.DependencyManagement;
using Rebel.Cms.Web.Model;

namespace Rebel.Cms.Web.Mvc.ModelBinders.FrontEnd
{
    [ModelBinderFor(typeof(IRebelRenderModel))]
    public class RenderModelBinder : IModelBinder
    {
        private readonly IRenderModelFactory _modelFactory;

        public RenderModelBinder(IRenderModelFactory modelFactory)
        {
            _modelFactory = modelFactory;
        }

        /// <summary>
        /// Binds the model to a value by using the specified controller context and binding context.
        /// </summary>
        /// <returns>
        /// The bound value.
        /// </returns>
        /// <param name="controllerContext">The controller context.</param><param name="bindingContext">The binding context.</param>
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var requestMatchesType = typeof(IRebelRenderModel).Equals(bindingContext.ModelType);

            if (requestMatchesType)
                return _modelFactory.Create(controllerContext.HttpContext, controllerContext.HttpContext.Request.RawUrl);

            return null;
        }
    }
}
