namespace Rebel.Framework.Configuration.Caching
{
    using System.Configuration;

    public class CachePolicyPickerRuleParameterCollection : ConfigurationElementCollection
    {
        public void Add(CachePolicyPickerRuleParameter param)
        {
            base.BaseAdd(param);
        }

        protected override string ElementName
        {
            get { return "param"; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new CachePolicyPickerRuleParameter();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            var castElement = ((CachePolicyPickerRuleParameter)element);
            return castElement.Type + castElement.ValueAsString.IfNullOrWhiteSpace(string.Empty);
        }
    }
}