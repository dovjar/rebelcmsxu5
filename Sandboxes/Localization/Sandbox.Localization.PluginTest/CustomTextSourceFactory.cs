using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Foundation.Localization.Configuration;
using Rebel.Foundation.Localization.Maintenance;
using Rebel.Foundation.Localization;
using System.Reflection;

namespace Sandbox.Localization.PluginTest
{
    public class CustomTextSourceFactory : ILocalizationTextSourceFactory
    {

        public ITextSource GetSource(Rebel.Foundation.Localization.TextManager textManager, Assembly referenceAssembly, string targetNamespace)
        {
            var source = new SimpleTextSource();
            source.Texts.Add(new LocalizedText
            {
                Namespace = targetNamespace,
                Key = "FactoryTest",
                Pattern = "I'm from a factory",
                Language = "en-US",
                Source = new TextSourceInfo { TextSource = source, ReferenceAssembly = referenceAssembly }
            });

            return source;
        }
    }
}
