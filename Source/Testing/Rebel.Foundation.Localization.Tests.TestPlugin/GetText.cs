using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Rebel.Foundation.Localization;

namespace Rebel.Foundation.Localization.Tests.TestPlugin
{
    public class L10n : Localization<ATestPlugin> { }

    public class ATestPlugin
    {
        public static string Get()
        {            
            return L10n.Get("Plugin.Key", new { Count = 10 });
        }
    }
}
