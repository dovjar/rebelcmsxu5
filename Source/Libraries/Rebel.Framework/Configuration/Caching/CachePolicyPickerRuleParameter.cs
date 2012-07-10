namespace Rebel.Framework.Configuration.Caching
{
    using System.Configuration;

    public class CachePolicyPickerRuleParameter : ConfigurationElement
    {

        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get { return (string)this["type"]; }
            set { base["type"] = value; }
        }

        [ConfigurationProperty("value", IsRequired = true)]
        public string ValueAsString
        {
            get { return (string)this["value"]; }
            set { base["value"] = value; }
        }
    }
}