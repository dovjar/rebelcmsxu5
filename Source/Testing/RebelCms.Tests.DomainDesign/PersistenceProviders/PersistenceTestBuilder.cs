using System;
using RebelCms.Framework.Persistence.Model;

namespace RebelCms.Tests.DomainDesign.PersistenceProviders
{
    public static class PersistenceTestBuilder
    {
        public static PersistenceAssertionBuilder<T> Create<T>(Action postWriteCallback) where T : AbstractEntity
        {
            return new PersistenceAssertionBuilder<T>(postWriteCallback);
        }
    }
}