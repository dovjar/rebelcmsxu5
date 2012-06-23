using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using RebelCms.Framework.EntityGraph.Domain.Entity;
using System.Collections.Generic;

namespace RebelCms.Framework.EntityGraph.Domain.ObjectModel
{
    internal sealed class LazyMappedIdentifierCollection<T> : Dictionary<IMappedIdentifier, Lazy<T>>
             where T : IEntity
    {
    }
}
