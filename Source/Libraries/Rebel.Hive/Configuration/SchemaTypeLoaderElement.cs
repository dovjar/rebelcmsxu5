using System.Configuration;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;

namespace Rebel.Hive.Configuration
{
    public class SchemaTypeLoaderElement : PersistenceTypeLoaderElement
    {
        [ConfigurationProperty("revisioning")]
        public RevisionTypeLoaderElement<EntitySchema> Revisioning
        {
            get { return (RevisionTypeLoaderElement<EntitySchema>)base["revisioning"]; }
            set { base["revisioning"] = value; }
        }
    }
}