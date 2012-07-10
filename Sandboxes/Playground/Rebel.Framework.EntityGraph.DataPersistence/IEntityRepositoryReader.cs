using System.Collections.Generic;
using Rebel.Framework.Data.Common;
using Rebel.Framework.Data.PersistenceSupport;
using Rebel.Framework.EntityGraph.Domain;
using Rebel.Framework.EntityGraph.Domain.Entity;
using Rebel.Framework.EntityGraph.Domain.Entity.Graph;

namespace Rebel.Framework.EntityGraph.DataPersistence
{
    public interface IEntityRepositoryReader<out TEnumeratorType, out TDeepEnumeratorType, TEntityType, out TDeepEntityType>
        : IRepositoryReader<TEnumeratorType, TDeepEnumeratorType, TEntityType, TDeepEntityType>,
        ISupportsProviderInjection
        where TEnumeratorType : IEnumerable<TEntityType>
        where TDeepEnumeratorType : IEnumerable<TDeepEntityType>
        where TEntityType : class, ITypedEntity
        where TDeepEntityType : class, ITypedEntityVertex
    {
        
    }
}