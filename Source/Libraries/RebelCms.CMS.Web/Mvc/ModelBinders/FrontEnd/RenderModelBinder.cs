using System.Web.Mvc;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.DependencyManagement;
using RebelCms.Cms.Web.Model;

namespace RebelCms.Cms.Web.Mvc.ModelBinders.FrontEnd
{
    [ModelBinderFor(typeof(IRebelCmsRenderModel))]
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
            var requestMatchesType = typeof(IRebelCmsRenderModel).Equals(bindingContext.ModelType);

            if (requestMatchesType)
                return _modelFactory.Create(controllerContext.HttpContext, controllerContext.HttpContext.Request.RawUrl);

            return null;
        }
    }
}
