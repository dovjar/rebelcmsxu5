namespace Rebel.Framework.Configuration.Caching
{
    using System.Configuration;

    public class CachePolicyCollection : ConfigurationElementCollection<string, CachePolicyElement>
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.AddRemoveClearMapAlternate; }
        }

        protected override string ElementName
        {
            get { return "policy"; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new CachePolicyElement();
        }

        protected override string GetElementKey(CachePolicyElement element)
        {
            return element.Name;
        }
    }
}