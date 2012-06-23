using RebelCms.Framework.Persistence.Model.Versioning;

namespace RebelCms.Hive.Configuration
{
    public class RevisionTypeLoaderElement<T> : PersistenceTypeLoaderElement
        where T : IVersionableEntity
    {
        
    }
}