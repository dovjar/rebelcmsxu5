using System.ComponentModel;
using System.Web.Mvc;
using RebelCms.Cms.Web.DependencyManagement;
using RebelCms.Cms.Web.Model.BackOffice.Editors;

namespace RebelCms.Cms.Web.Mvc.ModelBinders.BackOffice
{
    /// <summary>
    /// Model binder for DocumentTypeEditorModel
    /// </summary>
    [ModelBinderFor(typeof(DocumentTypeEditorModel))]    
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
