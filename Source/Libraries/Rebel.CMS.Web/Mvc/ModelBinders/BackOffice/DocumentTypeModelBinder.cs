using System.ComponentModel;
using System.Web.Mvc;
using Rebel.Cms.Web.DependencyManagement;
using Rebel.Cms.Web.Model.BackOffice.Editors;

namespace Rebel.Cms.Web.Mvc.ModelBinders.BackOffice
{
    /// <summary>
    /// Model binder for DocumentTypeEditorModel
    /// </summary>
    public class DocumentTypeModelBinder : StandardModelBinder
    {

        /// <summary>
        /// Binds custom properties that the default model binder wont be able to bind
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="bindingContext"></param>
        /// <param name="propertyDescriptor"></param>
        protected override void BindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor)
        {

            switch (propertyDescriptor.Name)
            {
                case "AllowedTemplates":
                case "AllowedChildren":
                case "InheritFrom":
                    this.BindSelectList(controllerContext, bindingContext, propertyDescriptor);
                    break;
                default:
                    base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
                    break;
            }

        }
    }

}
