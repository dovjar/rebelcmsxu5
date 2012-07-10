namespace Rebel.Framework.Configuration.Caching
{
    using System.Configuration;
    using System.Linq;
    using System.Text;

    public class CachePolicyPickerRuleCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new CachePolicyPickerRule();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            var castElement = (CachePolicyPickerRule)element;
            var sb = new StringBuilder();
            foreach (var parameter in castElement
                .Params
                .OfType<CachePolicyPickerRuleParameter>()
                .OrderBy(x => x.Type))
            {
                sb.Append(parameter.Type.ToLowerInvariant());
                sb.Append("-");
                sb.Append(parameter.ValueAsString.ToLowerInvariant());
            }
            return castElement.ForKeyType + castElement.Expression + sb.ToString();
        }
    }
}