namespace Umbraco.Framework.Configuration.Caching
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using Umbraco.Framework.Caching;

    public class LimitedLifetime : ConfigurationElement
    {
        protected Type _providerType;
        private CachePolicyElement _defaultPolicy;
        public const string ProviderXmlKey = "provider";

        internal void SetParent(General general)
        {
            ParentConfig = general;
        }
        protected General ParentConfig { get; private set; }

        public virtual Type GetKnownDefaultProviderType()
        {
            return typeof(PerHttpRequestCacheProvider);
        }

        public virtual IEnumerable<string> GetAlternativeDefaultProviderTypes()
        {
            yield break;
        }

        public virtual Type GetProviderType()
        {
            if (_providerType != null) return _providerType;

            var typeName = ProviderTypeName;
            if (typeName.IsNullOrWhiteSpace())
                return GetKnownDefaultProviderType();

            _providerType = Type.GetType(typeName, false, true) ?? GetKnownDefaultProviderType();
            if (_providerType == null)
            {
                foreach (var type in GetAlternativeDefaultProviderTypes())
                {
                    _providerType = Type.GetType(type, false, true);
                    if (_providerType != null) break;
                }
            }
            return _providerType;
        }

        [ConfigurationProperty(ProviderXmlKey)]
        public string ProviderTypeName
        {
            get { return (string)this[ProviderXmlKey]; }
            set
            {
                this[ProviderXmlKey] = value;
            }
        }

        [ConfigurationProperty("rules", IsRequired = true, IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(CachePolicyPickerRule), AddItemName = "rule", CollectionType = ConfigurationElementCollectionType.AddRemoveClearMapAlternate)]
        public CachePolicyPickerRuleCollection Rules
        {
            get { return (CachePolicyPickerRuleCollection)this["rules"]; }
        }

        public IEnumerable<CachePolicyPickerRule> GetRules()
        {
            return Rules.OfType<CachePolicyPickerRule>();
        }

        public CachePolicyElement GetPolicyElementFor(object cacheKey)
        {
            var matchingRule = Rules.OfType<CachePolicyPickerRule>().FirstOrDefault(x => x.KeyMatches(cacheKey));
            _defaultPolicy = _defaultPolicy
                             ??
                             ParentConfig.Policies.FirstOrDefault(x => x.Name.InvariantEquals(ParentConfig.Policies.DefaultPolicyName));

            if (matchingRule != null)
            {
                var getPolicy = ParentConfig.Policies.FirstOrDefault(x => x.Name.InvariantEquals(matchingRule.UsePolicyName));
                if (getPolicy != null)
                    return getPolicy;
            }

            return _defaultPolicy;
        }

        public ICachePolicy GetPolicyFor(object cacheKey)
        {
            return GetPolicyElementFor(cacheKey).IfNotNull(x => x.ToPolicy(), StaticCachePolicy.CreateDefault());
        }
    }
}