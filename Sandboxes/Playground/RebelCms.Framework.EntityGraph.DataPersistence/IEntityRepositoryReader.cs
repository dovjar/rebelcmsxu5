using System.Collections.Generic;
using RebelCms.Framework.Data.Common;
using RebelCms.Framework.Data.PersistenceSupport;
using RebelCms.Framework.EntityGraph.Domain;
using RebelCms.Framework.EntityGraph.Domain.Entity;
using RebelCms.Framework.EntityGraph.Domain.Entity.Graph;

namespace RebelCms.Framework.EntityGraph.DataPersistence
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