using System;
using Rebel.Framework.EntityGraph.Domain.Entity.Graph;

namespace Rebel.Framework.EntityGraph.Domain.EntityAdaptors
{
    public interface IUserEntityVertexAdaptor : ITypedEntityVertexAdaptor<IUserEntityVertexAdaptor>
    {
        string Username { get; set; }

        DateTime LastAuthenticationDate { get; set; }

        // etc.
    }
}