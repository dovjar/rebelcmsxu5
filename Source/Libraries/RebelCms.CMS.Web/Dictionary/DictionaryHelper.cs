using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using RebelCms.Cms.Web.Configuration.Languages;
using RebelCms.Cms.Web.Context;
using RebelCms.Framework.Persistence;
using RebelCms.Framework.Persistence.Model;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Persistence.Model.Constants.AttributeDefinitions;
using RebelCms.Framework.Persistence.Model.Constants.Schemas;
using RebelCms.Hive.ProviderGrouping;
using RebelCms.Hive;
using RebelCms.Hive.RepositoryTypes;

namespace RebelCms.Cms.Web.Dictionary
{
    public class DictionaryHelper
    {
        private IHiveManager _hiveManager;
        private IEnumerable<LanguageElement> _installedLanguages;

        public DictionaryHelper(IRebelCmsApplicationContext appContext)
            : this(appContext.Hive, appContext.Settings.Languages)
        { }

        public DictionaryHelper(IHiveManager hive, IEnumerable<LanguageElement> installedLanguages)
        {
            _hiveManager = hive;
            _installedLanguages = installedLanguages;
        }

        /// <summary>
        /// Gets a dictionary item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public DictionaryResult GetDictionaryItem(string key)
        {
            return GetDictionaryItem(key, Thread.CurrentThread.CurrentCulture.Name);
        }

        /// <summary>
        /// Gets a dictionary item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="language">The language.</param>
        /// <returns></returns>
        public DictionaryResult GetDictionaryItem(string key, string language)
        {
            var languageConfig = _installedLanguages.SingleOrDefault(x => x.IsoCode == language);
            if(languageConfig == null)
                return new DictionaryResult(false, key);

            var hive = _hiveManager.GetReader<IDictionaryStore>();
            using (var uow = hive.CreateReadonly())
            {
                var item = uow.Repositories.GetEntityByPath<TypedEntity>(FixedHiveIds.DictionaryVirtualRoot, key);
                if(item == null)
                    return new DictionaryResult(false, key);

                var val = item.Attribute<string>(DictionaryItemSchema.TranslationsAlias, languageConfig.IsoCode);
                if (!string.IsNullOrWhiteSpace(val))
                    return new DictionaryResult(true, key, val, languageConfig.IsoCode);

                foreach(var fallback in languageConfig.Fallbacks)
                {
                    val = item.Attribute<string>(DictionaryItemSchema.TranslationsAlias, fallback.IsoCode);
                    if (!string.IsNullOrWhiteSpace(val))
                        return new DictionaryResult(true, key, val, fallback.IsoCode);
                }
            }
            return new DictionaryResult(false, key);
        }

        /// <summary>
        /// Gets a dictionary item value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public string GetDictionaryItemValue(string key, string defaultValue = null)
        {
            var result = GetDictionaryItem(key);
            return result.Found ? result.Value : (defaultValue ?? "[" + key +"]");
        }

        /// <summary>
        /// Gets a dictionary item value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="language">The language.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public string GetDictionaryItemValueForLanguage(string key, string language, string defaultValue = null)
        {
            var result = GetDictionaryItem(key, language);
            return result.Found ? result.Value : (defaultValue ?? "[" + key + "]");
        }
    }
}
