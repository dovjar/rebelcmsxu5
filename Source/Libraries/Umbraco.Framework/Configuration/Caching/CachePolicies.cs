namespace Umbraco.Framework.Configuration.Caching
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;

    public class CachePolicies : ConfigurationElement, IEnumerable<CachePolicyElement>
    {
        [ConfigurationProperty("default", IsRequired = true)]
        public string DefaultPolicyName
        {
            get { return (string)this["default"]; }
            set { base["default"] = value; }
        }

        [ConfigurationProperty("", IsDefaultCollection = true, IsRequired = false)]
        public CachePolicyCollection Policies
        {
            get { return (CachePolicyCollection)base[""]; }
        }

        public IEnumerator<CachePolicyElement> GetEnumerator()
        {
            return Policies.OfType<CachePolicyElement>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}