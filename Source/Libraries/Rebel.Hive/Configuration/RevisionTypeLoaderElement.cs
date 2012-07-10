using Rebel.Framework.Persistence.Model.Versioning;

namespace Rebel.Hive.Configuration
{
    public class RevisionTypeLoaderElement<T> : PersistenceTypeLoaderElement
        where T : IVersionableEntity
    {
        
    }
}