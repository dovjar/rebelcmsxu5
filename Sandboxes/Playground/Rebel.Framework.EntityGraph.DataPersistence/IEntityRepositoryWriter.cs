using System.Collections.Generic;
using Rebel.Framework.Data.PersistenceSupport;
using Rebel.Framework.EntityGraph.Domain.Entity;
using Rebel.Framework.EntityGraph.Domain.Entity.Graph;

namespace Rebel.Framework.EntityGraph.DataPersistence
{
    public interface IEntityRepositoryWriter<TEnumeratorType, TDeepEnumeratorType, TEntityType, TDeepEntityType>
        : IRepositoryWriter<TEnumeratorType, TDeepEnumeratorType, TEntityType, TDeepEntityType>,
        ISupportsProviderInjection
        where TEnumeratorType : IEnumerable<TEntityType>
        where TDeepEnumeratorType : IEnumerable<TDeepEntityType>
        where TEntityType : class, ITypedEntity
        where TDeepEntityType : class, ITypedEntityVertex
    {
    }
}