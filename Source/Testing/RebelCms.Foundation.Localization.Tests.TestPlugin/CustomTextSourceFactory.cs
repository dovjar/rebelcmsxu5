 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RebelCms.Foundation.Localization.Configuration;
using RebelCms.Foundation.Localization.Maintenance;
using RebelCms.Foundation.Localization;
using System.Reflection;

namespace RebelCms.Foundation.Localization.Tests.TestPlugin
{
    public class CustomTextSourceFactory : ILocalizationTextSourceFactory
    {

        public ITextSource GetSource(RebelCms.Foundation.Localization.TextManager textManager, Assembly referenceAssembly, string targetNamespace)
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
