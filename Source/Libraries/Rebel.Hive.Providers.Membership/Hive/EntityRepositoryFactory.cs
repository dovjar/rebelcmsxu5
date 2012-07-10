using System;
using System.Collections.Generic;
using System.Web.Security;
using Rebel.Framework;
using Rebel.Framework.Context;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.ProviderSupport._Revised;
using Rebel.Hive.ProviderSupport;
using Rebel.Hive.Providers.Membership.Config;

namespace Rebel.Hive.Providers.Membership.Hive
{
    [DemandsDependencies(typeof(MembershipDemandBuilder))]
    public class EntityRepositoryFactory : AbstractEntityRepositoryFactory
    {
        public DependencyHelper MembershipDependencyHelper { get { return base.DependencyHelper as DependencyHelper; } }
        //public SchemaRepositoryFactory MembershipSchemaRepositoryFactory { get { return base.SchemaRepositoryFactory as SchemaRepositoryFactory; } }

        /// <summary>
        /// Internal constructor for testing
        /// </summary>
        /// <param name="providerMetadata"></param>
        /// <param name="revisionRepositoryFactory"></param>
        /// <param name="schemaRepositoryFactory"></param>
        /// <param name="frameworkContext"></param>
        /// <param name="membershipProviders"></param>
        /// <param name="configuredProviders"></param>
        internal EntityRepositoryFactory(
            ProviderMetadata providerMetadata,
            AbstractRevisionRepositoryFactory<TypedEntity> revisionRepositoryFactory,
            AbstractSchemaRepositoryFactory schemaRepositoryFactory,
            IFrameworkContext frameworkContext,
            Lazy<IEnumerable<MembershipProvider>> membershipProviders,
            IEnumerable<ProviderElement> configuredProviders)
            : base(providerMetadata, revisionRepositoryFactory, schemaRepositoryFactory, frameworkContext, new DependencyHelper(configuredProviders, membershipProviders, providerMetadata))
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="providerMetadata"></param>
        /// <param name="revisionRepositoryFactory"></param>
        /// <param name="schemaRepositoryFactory"></param>
        /// <param name="frameworkContext"></param>
        /// <param name="dependencyHelper"></param>
        public EntityRepositoryFactory(
            ProviderMetadata providerMetadata,
            AbstractRevisionRepositoryFactory<TypedEntity> revisionRepositoryFactory,
            AbstractSchemaRepositoryFactory schemaRepositoryFactory,
            IFrameworkContext frameworkContext,
            ProviderDependencyHelper dependencyHelper)
            : base(providerMetadata, revisionRepositoryFactory, schemaRepositoryFactory, frameworkContext, dependencyHelper)
        {
        }

        protected override void DisposeResources()
        {
            RevisionRepositoryFactory.Dispose();
            SchemaRepositoryFactory.Dispose();
            DependencyHelper.Dispose();
        }

        public override AbstractReadonlyEntityRepository GetReadonlyRepository()
        {
            return GetRepository();
        }

        public override AbstractEntityRepository GetRepository()
        {
            return new EntityRepository(
                ProviderMetadata, 
                FrameworkContext,
                MembershipDependencyHelper.MembershipProviders.Value,
                MembershipDependencyHelper.ConfiguredProviders);
        }
    }
}
