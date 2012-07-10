using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rebel.Framework.Dynamics.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class DynamicExtensionAttribute : Attribute
    {
        public string Name { get; set; }

        public DynamicExtensionAttribute()
            : this(string.Empty)
        { }

        public DynamicExtensionAttribute(string name)
        {
            Name = name;
        }
    }
}
