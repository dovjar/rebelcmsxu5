using System;
using System.IO;
using System.Web.Mvc;
using Rhino.Mocks;
using RebelCms.Cms;
using RebelCms.Cms.Packages.DevDataset;
using RebelCms.Cms.Web;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.DependencyManagement;
using RebelCms.Cms.Web.Mapping;
using RebelCms.Cms.Web.Model;
using RebelCms.Cms.Web.Model.BackOffice.Editors;
using RebelCms.Cms.Web.Mvc.Metadata;
using RebelCms.Cms.Web.Mvc.ModelBinders;
using RebelCms.Cms.Web.Mvc.ModelBinders.BackOffice;
using RebelCms.Framework;
using RebelCms.Framework.Persistence;
using RebelCms.Framework.Persistence.ProviderSupport._Revised;
using RebelCms.Framework.TypeMapping;
using RebelCms.Hive;
using RebelCms.Hive.Configuration;
using RebelCms.Hive.ProviderGrouping;
using RebelCms.Hive.Providers.IO;
using RebelCms.Hive.ProviderSupport;
using RebelCms.Hive.RepositoryTypes;
using RebelCms.Tests.Cms.Stubs;
using RebelCms.Tests.Extensions;
using RebelCms.Tests.Extensions.Stubs;

namespace RebelCms.Tests.Cms
{
    /// <summary>
    /// An abstract test class that ensures that all framework/application contexts are setup properly
    /// </summary>
    public abstract class StandardWebTest
    {
        protected FakeRebelCmsApplicationContext RebelCmsApplicationContext { get; private set; }
        protected DevDataset DevDataset { get; private set; }
        protected FakeFrameworkContext FrameworkContext { get; private set; }
        protected FixedPropertyEditors FixedPropertyEditors { get; private set; }

        /// <summary>
        /// Creates a new routable request context with everything wired up
        /// </summary>
        protected virtual IBackOfficeRequestContext GetBackOfficeRequestContext()
        {
            return new FakeBackOfficeRequestContext(RebelCmsApplicationContext);
        }

        protected virtual void Init()
        {
            var attributeTypeRegistry = new CmsAttributeTypeRegistry();
            AttributeTypeRegistry.SetCurrent(attributeTypeRegistry);

            FrameworkContext = new FakeFrameworkContext();

            var hive = CreateHiveManager();
            
            RebelCmsApplicationContext = CreateApplicationContext(hive);

            var resolverContext = new MockedMapResolverContext(FrameworkContext, hive, new MockedPropertyEditorFactory(RebelCmsApplicationContext), new MockedParameterEditorFactory());
            var webmModelMapper = new CmsModelMapper(resolverContext);

            FrameworkContext.SetTypeMappers(new FakeTypeMapperCollection(new AbstractMappingEngine[] { webmModelMapper, new FrameworkModelMapper(FrameworkContext) }));

            DevDataset = DemoDataHelper.GetDemoData(RebelCmsApplicationContext, attributeTypeRegistry);
            FixedPropertyEditors = new FixedPropertyEditors(RebelCmsApplicationContext);

            //ensure model binders
            ModelBinders.Binders.Remove(typeof(HiveId));
            ModelBinders.Binders.Add(typeof(HiveId), new HiveIdModelBinder());
            ModelBinders.Binders.Remove(typeof(DocumentTypeEditorModel));
            ModelBinders.Binders.Add(typeof(DocumentTypeEditorModel), new DocumentTypeModelBinder());
            ModelBinders.Binders.Remove(typeof(SizeModelBinder));
            ModelBinders.Binders.Add(typeof(SizeModelBinder), new SizeModelBinder());

            //set the model meta data provider
            ModelMetadataProviders.Current = new RebelCmsModelMetadataProvider();
        }
        
        /// <summary>
        /// Returns a FakeRebelCmsApplicationContext by default with the Root node created
        /// </summary>
        /// <param name="hive"></param>
        /// <returns></returns>
        protected virtual FakeRebelCmsApplicationContext CreateApplicationContext(IHiveManager hive)
        {
            return new FakeRebelCmsApplicationContext(hive);
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