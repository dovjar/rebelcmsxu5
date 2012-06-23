using System.Configuration;
using RebelCms.Framework.Persistence.Model.Attribution.MetaData;

namespace RebelCms.Hive.Configuration
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