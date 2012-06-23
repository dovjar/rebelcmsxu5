using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RebelCms.Cms.Web.Macros
{
    /// <summary>
    /// Identifies a default RebelCms macro engine that is shipped with the framework
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal class RebelCmsMacroEngineAttribute : Attribute
    {

    }
}
