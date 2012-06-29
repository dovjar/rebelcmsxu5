using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Framework.Dynamics.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class DynamicFieldExtensionAttribute : Attribute
    {
        public string PropertyEditorId { get; set; }
        public string Name { get; set; }

        public DynamicFieldExtensionAttribute(string propertyEditorId)
            : this(propertyEditorId, string.Empty)
        { }

        public DynamicFieldExtensionAttribute(string propertyEditorId, string name)
        {
            PropertyEditorId = propertyEditorId;
            Name = name;
        }
    }
}
