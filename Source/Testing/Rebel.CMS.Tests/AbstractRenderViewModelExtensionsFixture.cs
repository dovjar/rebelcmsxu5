using System;
using NUnit.Framework;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Mapping;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.TypeMapping;
using Rebel.Hive;
using Rebel.Hive.Configuration;
using Rebel.Hive.RepositoryTypes;
using Rebel.Tests.Extensions;
using System.Linq;
using Rebel.Cms.Web.Security;
using Rebel.Framework.Security;
using Rebel.Framework.Security.Configuration;
using Rebel.Framework.Security.Model.Entities;

namespace Rebel.Tests.Cms
{
    
    [TestFixture]
    public class AbstractRenderViewModelExtensionsFixture
    {
        protected IRebelApplicationContext AppContext { get; private set; }
        protected HiveManager HiveManager { get; private set; }
        protected IMembershipService<Member> MembershipService { get; private set; }
        protected IPublicAccessService PublicAccessService { get; private set; }

        private ProviderMappingGroup _singleProvider;
        protected NhibernateTestSetupHelper Setup;        
        private MockedMapResolverContext _resolverContext;

        [SetUp]
        public void TestSetup()
        {
            this.Setup = new NhibernateTestSetupHelper();
            this._singleProvider = new ProviderMappingGroup("default", new WildcardUriMatch(new Uri("content://")), this.Setup.ReadonlyProviderSetup, this.Setup.ProviderSetup, this.Setup.FakeFrameworkContext);
            this.HiveManager = new HiveManager(this._singleProvider, this._singleProvider.FrameworkContext);

            this.AppContext = new FakeRebelApplicationContext(this.HiveManager, false);

            this._resolverContext = new MockedMapResolverContext(this.HiveManager.FrameworkContext, this.HiveManager, new MockedPropertyEditorFactory(this.AppContext), new MockedParameterEditorFactory());

            //mappers
            var cmsModelMapper = new CmsModelMapper(this._resolverContext);
            var persistenceToRenderModelMapper = new RenderTypesModelMapper(this._resolverContext);
            
            this.Setup.FakeFrameworkContext.SetTypeMappers(new FakeTypeMapperCollection(new AbstractMappingEngine[] { cmsModelMapper, persistenceToRenderModelMapper }));

            var membersMembershipProvider = new MembersMembershipProvider { AppContext = AppContext };
            MembershipService = new MembershipService<Member, MemberProfile>(AppContext.FrameworkContext, HiveManager,
                "security://member-profiles", "security://member-groups", Framework.Security.Model.FixedHiveIds.MemberProfileVirtualRoot,
                membersMembershipProvider, Enumerable.Empty<MembershipProviderElement>());

            PublicAccessService = new PublicAccessService(HiveManager, MembershipService, AppContext.FrameworkContext);
        }

        protected TypedEntity AddChildNode(TypedEntity parent, TypedEntity child, int sortOrder = 0)
        {
            using (var uow = this.HiveManager.OpenWriter<IContentStore>())
            {
                parent.RelationProxies.EnlistChild(child, FixedRelationTypes.DefaultRelationType, sortOrder);
                uow.Repositories.AddOrUpdate(parent);
                uow.Repositories.AddOrUpdate(child);
                uow.Complete();
            }
            return child;
        }

        protected TypedEntity AddChildNodeWithId(TypedEntity parent, HiveId childGuid, int sortOrder = 0)
        {
            var child = HiveModelCreationHelper.MockTypedEntity(childGuid);
            return AddChildNode(parent, child, sortOrder);
        }
    }
}