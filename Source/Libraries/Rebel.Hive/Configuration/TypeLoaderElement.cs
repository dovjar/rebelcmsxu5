using System.Configuration;
using Rebel.Framework.Persistence.Model;

namespace Rebel.Hive.Configuration
{
    public class TypeLoaderElement : PersistenceTypeLoaderElement
    {
        [ConfigurationProperty("revisioning")]
        public RevisionTypeLoaderElement<TypedEntity> Revisioning
        {
            get { return (RevisionTypeLoaderElement<TypedEntity>)base["revisioning"]; }
            set { base["revisioning"] = value; }
        }

        [ConfigurationProperty("schema")]
        public SchemaTypeLoaderElement Schema
        {
            get { return (SchemaTypeLoaderElement)base["schema"]; }
            set { base["schema"] = value; }
        }
    }
}
