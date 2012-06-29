using System;
using NUnit.Framework;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Mapping;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.TypeMapping;
using Umbraco.Hive;
using Umbraco.Hive.Configuration;
using Umbraco.Hive.RepositoryTypes;
using Umbraco.Tests.Extensions;
using System.Linq;
using Umbraco.Cms.Web.Security;
using Umbraco.Framework.Security;
using Umbraco.Framework.Security.Configuration;
using Umbraco.Framework.Security.Model.Entities;

namespace Umbraco.Tests.Cms
{
    
    [TestFixture]
    public class AbstractRenderViewModelExtensionsFixture
    {
        protected IUmbracoApplicationContext AppContext { get; private set; }
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

            this.AppContext = new FakeUmbracoApplicationContext(this.HiveManager, false);

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