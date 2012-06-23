using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RebelCms.Framework.Localization.Configuration;
using RebelCms.Framework.Localization.Maintenance;
using RebelCms.Framework.Localization;
using System.Reflection;

namespace TestPlugin.Texts
{
    public class TextFactory : ILocalizationTextSourceFactory
    {
        public ITextSource GetSource(TextManager textManager, Assembly referenceAssembly, string targetNamespace)
        {
            var source = new SimpleTextSource();
            source.Texts.Add(new LocalizedText
            {
                Namespace = targetNamespace,
                Key = "Tulips1",
                Pattern = "resource:TestPlugin.Koala.jpg",
                Language = "da-DK",
                Source = new TextSourceInfo { TextSource = source, ReferenceAssembly = referenceAssembly }
            });

            return source;
        }
    }
}
