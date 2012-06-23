using NHibernate;
using RebelCms.Framework.Context;
using RebelCms.Framework.Persistence.Model;
using RebelCms.Framework.Persistence.NHibernate.Dependencies;
using RebelCms.Framework.Persistence.ProviderSupport._Revised;
using RebelCms.Hive.ProviderSupport;

namespace RebelCms.Framework.Persistence.NHibernate
{
    public class RevisionRepositoryFactory : AbstractRevisionRepositoryFactory<TypedEntity>
    {
        public DependencyHelper NhDependencyHelper { get { return base.DependencyHelper as DependencyHelper; } }

        internal RevisionRepositoryFactory(ProviderMetadata providerMetadata, IFrameworkContext frameworkContext, ISession singleProvidedSession, bool leaveSessionOpenOnDispose)
            : base(providerMetadata, frameworkContext, new DependencyHelper(new NhFactoryHelper(null, singleProvidedSession, leaveSessionOpenOnDispose, false, frameworkContext), providerMetadata))
        {
        }

        public RevisionRepositoryFactory(ProviderMetadata providerMetadata, IFrameworkContext frameworkContext, ProviderDependencyHelper dependencyHelper)
            : base(providerMetadata, frameworkContext, dependencyHelper)
        {
        }

        protected override void DisposeResources()
        {
            NhDependencyHelper.Dispose();
        }

        public override AbstractReadonlyRevisionRepository<TypedEntity> GetReadonlyRepository()
        {
            return CreateRevisionRepository(true);
        }

        /// <summary>
        /// Create a new repo using an existing transaction
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="isReadOnly"> </param>
        /// <returns></returns>
        public AbstractRevisionRepository<TypedEntity> GetRepository(NhProviderTransaction transaction, bool isReadOnly)
        {
            var session = NhDependencyHelper.FactoryHelper.GetSessionFromTransaction(transaction, isReadOnly);
            return new RevisionRepository(ProviderMetadata, transaction, session, FrameworkContext, isReadOnly);
        }

        public override AbstractRevisionRepository<TypedEntity> GetRepository()
        {
            return CreateRevisionRepository(false);
        }

        protected virtual AbstractRevisionRepository<TypedEntity> CreateRevisionRepository(bool isReadOnly)
        {
            ISession session;
            var transaction = NhDependencyHelper.FactoryHelper.GenerateSessionAndTransaction(isReadOnly, out session);
            return new RevisionRepository(ProviderMetadata, transaction, session, FrameworkContext, isReadOnly);
        }
    }
}