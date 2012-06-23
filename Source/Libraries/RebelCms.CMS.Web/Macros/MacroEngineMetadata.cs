using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RebelCms.Cms.Web.Model.BackOffice;
using RebelCms.Framework;

namespace RebelCms.Cms.Web.Macros
{
    public class MacroEngineMetadata : PluginMetadataComposition
    {
        public MacroEngineMetadata(IDictionary<string, object> obj)
            : base(obj)
        {
        }


        /// <summary>
        /// Whether or not this is an built-in RebelCms macro engine
        /// </summary>
        public bool IsInternalRebelCmsEngine { get; set; }

        /// <summary>
        /// Gets the name of the MacroEngine
        /// </summary>
        public string EngineName { get; set; }

    }
}
