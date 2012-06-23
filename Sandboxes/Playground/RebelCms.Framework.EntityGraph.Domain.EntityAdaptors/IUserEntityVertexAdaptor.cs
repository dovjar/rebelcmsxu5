using System;
using RebelCms.Framework.EntityGraph.Domain.Entity.Graph;

namespace RebelCms.Framework.EntityGraph.Domain.EntityAdaptors
{
    public interface IUserEntityVertexAdaptor : ITypedEntityVertexAdaptor<IUserEntityVertexAdaptor>
    {
        string Username { get; set; }

        DateTime LastAuthenticationDate { get; set; }

        // etc.
    }
}