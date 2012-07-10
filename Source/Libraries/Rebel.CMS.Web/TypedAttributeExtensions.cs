using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Framework;
using Rebel.Framework.Dynamics.Attributes;
using Rebel.Framework.Persistence.Model.Attribution;

namespace Rebel.Cms.Web
{
    [DynamicExtensions]
    public static class TypedAttributeExtensions
    {
        [DynamicFieldExtension(CorePluginConstants.TreeNodePickerPropertyEditorId)]
        public static bool HasValue(this TypedAttribute attr)
        {
            HiveId value = HiveId.Parse(attr.DynamicValue);
            return !value.IsNullValueOrEmpty();
        }
    }
}
