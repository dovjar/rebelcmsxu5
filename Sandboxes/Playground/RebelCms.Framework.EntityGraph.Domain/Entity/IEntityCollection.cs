using System.Collections.Generic;

namespace RebelCms.Framework.EntityGraph.Domain.Entity
{
    /// <summary>
    /// Provides a simple 1-dimensional list of entities
    /// </summary>
    public interface IEntityCollection : IDictionary<IMappedIdentifier, IEntity>, IEnumerable<IEntity>
    {
    }
}