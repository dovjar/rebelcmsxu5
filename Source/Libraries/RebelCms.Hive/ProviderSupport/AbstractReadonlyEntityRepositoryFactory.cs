using System;
using RebelCms.Framework.Context;
using RebelCms.Framework.Persistence.Model;
using RebelCms.Framework.Persistence.ProviderSupport._Revised;

namespace RebelCms.Hive.ProviderSupport
{
    public abstract class AbstractReadonlyEntityRepositoryFactory
        : AbstractReadonlyRepositoryFactory<AbstractReadonlyEntityRepository>, IProviderReadonlyEntityRepositoryFactory
    {
        protected AbstractReadonlyEntityRepositoryFactory(ProviderMetadata providerMetadata, 
            AbstractReadonlySchemaRepositoryFactory schemaRepositoryFactory, 
            AbstractReadonlyRevisionRepositoryFactory<TypedEntity> revisionRepositoryFactory, 
            IFrameworkContext frameworkContext, 
            ProviderDependencyHelper dependencyHelper)
            : base(providerMetadata, frameworkContext, dependencyHelper)
        {
            SchemaRepositoryFactory = schemaRepositoryFactory;
            RevisionRepositoryFactory = revisionRepositoryFactory;
        }

        /// <summary>
        /// Gets or sets the schema session factory.
        /// </summary>
        /// <value>The schema session factory.</value>
        public AbstractReadonlySchemaRepositoryFactory SchemaRepositoryFactory { get; protected set; }

        /// <summary>
        /// Gets or sets the revision session factory.
        /// </summary>
        /// <value>The revision session factory.</value>
        public AbstractReadonlyRevisionRepositoryFactory<TypedEntity> RevisionRepositoryFactory { get; protected set; }
    }
}