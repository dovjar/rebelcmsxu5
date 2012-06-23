using System.Configuration;
using RebelCms.Framework.Configuration;

namespace RebelCms.Hive.Configuration
{
    [ConfigurationCollection(typeof(ReadWriterTypeReferenceElement))]
    public abstract class AbstractTypeReferenceElementCollection<T> : ConfigurationElementCollection<string, T> 
        where T : ReadWriterTypeReferenceElement, new()
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
        }

        protected abstract override string ElementName { get; }

        protected override string GetElementKey(T element)
        {
            return element.ProviderKey;
        }
    }
}