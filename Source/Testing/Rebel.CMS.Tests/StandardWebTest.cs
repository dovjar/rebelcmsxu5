using System.Web.Mvc;
using Rhino.Mocks;
using Rebel.Cms;
using Rebel.Cms.Packages.DevDataset;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.DependencyManagement;
using Rebel.Cms.Web.Mapping;
using Rebel.Cms.Web.Model;
using Rebel.Cms.Web.Model.BackOffice.Editors;
using Rebel.Cms.Web.Mvc.Metadata;
using Rebel.Cms.Web.Mvc.ModelBinders;
using Rebel.Cms.Web.Mvc.ModelBinders.BackOffice;
using Rebel.Framework;
using Rebel.Framework.Persistence;
using Rebel.Framework.Persistence.ProviderSupport._Revised;
using Rebel.Framework.TypeMapping;
using Rebel.Hive;
using Rebel.Hive.Configuration;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.Providers.IO;
using Rebel.Hive.ProviderSupport;
using Rebel.Hive.RepositoryTypes;
using Rebel.Tests.Extensions;
using Rebel.Tests.Extensions.Stubs;

namespace Rebel.Tests.Cms
{
    /// <summary>
    /// An abstract test class that ensures that all framework/application contexts are setup properly
    /// </summary>
    public abstract class StandardWebTest
    {
        protected FakeRebelApplicationContext RebelApplicationContext { get; private set; }
        protected DevDataset DevDataset { get; private set; }
        protected FakeFrameworkContext FrameworkContext { get; private set; }
        protected FixedPropertyEditors FixedPropertyEditors { get; private set; }

        /// <summary>
        /// Creates a new routable request context with everything wired up
        /// </summary>
        protected virtual IBackOfficeRequestContext GetBackOfficeRequestContext()
        {
            return new FakeBackOfficeRequestContext(RebelApplicationContext);
        }

        protected virtual void Init()
        {
            var attributeTypeRegistry = new CmsAttributeTypeRegistry();
            AttributeTypeRegistry.SetCurrent(attributeTypeRegistry);

            FrameworkContext = new FakeFrameworkContext();

            var hive = CreateHiveManager();
            
            RebelApplicationContext = CreateApplicationContext(hive);

            var resolverContext = new MockedMapResolverContext(FrameworkContext, hive, new MockedPropertyEditorFactory(RebelApplicationContext), new MockedParameterEditorFactory());
            var webmModelMapper = new CmsModelMapper(resolverContext);

            FrameworkContext.SetTypeMappers(new FakeTypeMapperCollection(new AbstractMappingEngine[] { webmModelMapper, new FrameworkModelMapper(FrameworkContext) }));

            DevDataset = DemoDataHelper.GetDemoData(RebelApplicationContext, attributeTypeRegistry);
            FixedPropertyEditors = new FixedPropertyEditors(RebelApplicationContext);

            //ensure model binders
            ModelBinders.Binders.Remove(typeof(HiveId));
            ModelBinders.Binders.Add(typeof(HiveId), new HiveIdModelBinder());
            ModelBinders.Binders.Remove(typeof(DocumentTypeEditorModel));
            ModelBinders.Binders.Add(typeof(DocumentTypeEditorModel), new DocumentTypeModelBinder());
            ModelBinders.Binders.Remove(typeof(SizeModelBinder));
            ModelBinders.Binders.Add(typeof(SizeModelBinder), new SizeModelBinder());

            //set the model meta data provider
            ModelMetadataProviders.Current = new RebelModelMetadataProvider();
        }
        
        /// <summary>
        /// Returns a FakeRebelApplicationContext by default with the Root node created
        /// </summary>
        /// <param name="hive"></param>
        /// <returns></returns>
        protected virtual FakeRebelApplicationContext CreateApplicationContext(IHiveManager hive)
        {
            return new FakeRebelApplicationContext(hive);
        }

        /// <summary>
        /// Returns a HiveManager configured with an in memory NH provider by default
        /// </summary>
        /// <returns></returns>
        protected virtual IHiveManager CreateHiveManager()
        {            
            return FakeHiveCmsManager.NewWithNhibernate(new[] { FakeHiveCmsManager.CreateFakeTemplateMappingGroup(FrameworkContext)}, FrameworkContext);
        }
    }
}