using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Foundation.Localization.Maintenance;
using Rebel.Foundation.Localization.Configuration;
using Rebel.Foundation.Localization;
using System.Reflection;

namespace Rebel.Foundation.Localization.Tests.TestPlugin
{
    public class MutatingTextSourceFactory : ILocalizationTextSourceFactory
    {

        private static SimpleTextSource _source = new SimpleTextSource();
        private static string _targetNamespace;
        private static Assembly _referenceAssembly;

        public static void Mutate(string newValue)
        {
            using (_source.BeginUpdate())
            {
                _source.Texts.Clear();
                _source.Texts.Add(new LocalizedText
                {
                    Namespace = _targetNamespace,
                    Key = "Mutating",
                    Pattern = newValue,
                    Language = "en-US",
                    Source = new TextSourceInfo { TextSource = _source, ReferenceAssembly = _referenceAssembly }
                });
            }
        }



        public ITextSource GetSource(Rebel.Foundation.Localization.TextManager textManager, Assembly referenceAssembly, string targetNamespace)
        {
            _targetNamespace = targetNamespace;
            _referenceAssembly = referenceAssembly;
            return _source;
        }
    }
}
