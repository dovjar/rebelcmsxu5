using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Cms.Web.Model.BackOffice;
using Rebel.Framework;

namespace Rebel.Cms.Web.Macros
{
    public class MacroEngineMetadata : PluginMetadataComposition
    {
        public MacroEngineMetadata(IDictionary<string, object> obj)
            : base(obj)
        {
        }


        /// <summary>
        /// Whether or not this is an built-in Rebel macro engine
        /// </summary>
        public bool IsInternalRebelEngine { get; set; }

        /// <summary>
        /// Gets the name of the MacroEngine
        /// </summary>
        public string EngineName { get; set; }

    }
}
