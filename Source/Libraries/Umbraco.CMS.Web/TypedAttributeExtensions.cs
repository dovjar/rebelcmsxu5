using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Framework;
using Umbraco.Framework.Dynamics.Attributes;
using Umbraco.Framework.Persistence.Model.Attribution;

namespace Umbraco.Cms.Web
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
