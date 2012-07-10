using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rebel.Cms.Web.Macros
{
    /// <summary>
    /// Identifies a default Rebel macro engine that is shipped with the framework
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal class RebelMacroEngineAttribute : Attribute
    {

    }
}
