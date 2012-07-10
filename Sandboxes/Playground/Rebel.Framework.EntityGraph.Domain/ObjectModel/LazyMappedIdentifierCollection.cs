using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using Rebel.Framework.EntityGraph.Domain.Entity;
using System.Collections.Generic;

namespace Rebel.Framework.EntityGraph.Domain.ObjectModel
{
    internal sealed class LazyMappedIdentifierCollection<T> : Dictionary<IMappedIdentifier, Lazy<T>>
             where T : IEntity
    {
    }
}
