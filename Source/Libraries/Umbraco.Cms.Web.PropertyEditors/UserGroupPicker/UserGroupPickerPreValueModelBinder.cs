using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Cms.Web.PropertyEditors.TreeNodePicker;

namespace Umbraco.Cms.Web.PropertyEditors.UserGroupPicker
{
    public class UserGroupPickerPreValueModelBinder : DefaultModelBinder
    {
        protected override void BindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor)
        {
            var model = (UserGroupPickerPreValueModel)bindingContext.Model;

            switch (propertyDescriptor.Name)
            {
                case "AvailableTypes":
                    var types = model.GetAvailableTypes().ToArray();
                    var valueName = string.Concat(bindingContext.ModelName, ".", propertyDescriptor.Name);
                    var value = bindingContext.ValueProvider.GetValue(valueName);
                    if (value != null)
                    {
                        var item = types.SingleOrDefault(x => x.Value == value.AttemptedValue);
                        if (item != null)
                            item.Selected = true;
                    }
                    propertyDescriptor.SetValue(bindingContext.Model, types);
                    break;
                default:
                    base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
                    break;
            }

        }
    }
}
