using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rebel.Framework.Dynamics.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DynamicExtensionsAttribute : Attribute
    { }
}
