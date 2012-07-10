using System.Web.Mvc;
using Rebel.Cms.Web.DependencyManagement;
using Rebel.Framework;

namespace Rebel.Cms.Web.Mvc.ModelBinders.BackOffice
{
    [ModelBinderFor(typeof (HiveId?))]
    public class NullableHiveIdModelBinder : DefaultModelBinder
    {
        /// <summary>
        /// Binds the model
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="bindingContext"></param>
        /// <returns></returns>
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            return HiveIdModelBinder.PerformBindModel(controllerContext, bindingContext);
        }

    }
}