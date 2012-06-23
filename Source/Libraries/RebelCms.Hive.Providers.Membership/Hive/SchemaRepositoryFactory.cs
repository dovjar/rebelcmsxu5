//using System;
//using System.Collections.Generic;
//using System.Web.Security;
//using RebelCms.Framework.Context;
//using RebelCms.Framework.Persistence.Model.Attribution.MetaData;
//using RebelCms.Framework.Persistence.ProviderSupport._Revised;
//using RebelCms.Hive.ProviderSupport;

//namespace RebelCms.Hive.Providers.Membership.Hive
//{
//    public class SchemaRepositoryFactory : AbstractSchemaRepositoryFactory
//    {
//        public DependencyHelper MembershipDependencyHelper { get { return base.DependencyHelper as DependencyHelper; } }

//        /// <summary>
//        /// Constructor used for testing
//        /// </summary>
//        /// <param name="providerMetadata"></param>
//        /// <param name="revisionRepositoryFactory"></param>
//        /// <param name="frameworkContext"></param>
//        /// <param name="membershipProviders"></param>
//        internal SchemaRepositoryFactory(
//            ProviderMetadata providerMetadata,
//            AbstractRevisionRepositoryFactory<EntitySchema> revisionRepositoryFactory,
//            IFrameworkContext frameworkContext,
//            Lazy<IEnumerable<MembershipProvider>> membershipProviders)
//            : base(providerMetadata, revisionRepositoryFactory, frameworkContext, new DependencyHelper(membershipProviders, providerMetadata))
//        {
//        }

//        /// <summary>
//        /// Constructor
//        /// </summary>
//        /// <param name="providerMetadata"></param>
//        /// <param name="revisionRepositoryFactory"></param>
//        /// <param name="frameworkContext"></param>
//        /// <param name="dependencyHelper"></param>
//        public SchemaRepositoryFactory(
//            ProviderMetadata providerMetadata,
//            AbstractRevisionRepositoryFactory<EntitySchema> revisionRepositoryFactory, 
//            IFrameworkContext frameworkContext, 
//            ProviderDependencyHelper dependencyHelper)
//            : base(providerMetadata, revisionRepositoryFactory, frameworkContext, dependencyHelper)
//        {
//        }

//        protected override void DisposeResources()
//        {
//            RevisionRepositoryFactory.Dispose();
//            MembershipDependencyHelper.Dispose();
//        }

//        public override AbstractReadonlySchemaRepository GetReadonlyRepository()
//        {
//            return GetRepository();
//        }


//        public override AbstractSchemaRepository GetRepository()
//        {
//            return new SchemaRepository(ProviderMetadata, FrameworkContext);

//        }
//    }
//}
