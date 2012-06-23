using System;
using System.Linq;
using NUnit.Framework;
using RebelCms.Cms;
using RebelCms.Cms.Web;
using RebelCms.Cms.Web.Model.BackOffice.Editors;
using RebelCms.Cms.Web.Model.BackOffice.PropertyEditors;
using RebelCms.Cms.Web.PropertyEditors.RebelCmsSystemEditors.NodeName;
using RebelCms.Cms.Web.PropertyEditors.RebelCmsSystemEditors.SelectedTemplate;
using RebelCms.Cms.Web.System;
using RebelCms.Framework;
using RebelCms.Framework.Persistence.Model;
using RebelCms.Framework.Persistence.Model.Attribution.MetaData;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Persistence.Model.Constants.AttributeDefinitions;
using RebelCms.Framework.Persistence.Model.Constants.Schemas;
using RebelCms.Framework.Persistence.Model.Constants.SerializationTypes;
using RebelCms.Framework.Persistence.Model.Versioning;
using RebelCms.Hive.RepositoryTypes;
using RebelCms.Tests.Extensions;
using RebelCms.Framework.Persistence;
using RebelCms.Hive;
using RebelCms.Tests.Extensions.Stubs.PropertyEditors;

namespace RebelCms.Tests.Cms.Editors
{
    /// <summary>
    /// Base class for controller tests needing a content hive provider
    /// </summary>
    [TestFixture]
    public abstract class AbstractContentControllerTest : StandardWebTest
    {
        [TestFixtureSetUp]
        public static void TestSetup()
        {
            DataHelper.SetupLog4NetForTests();
        }
        
        /// <summary>
        /// initialize tests
        /// </summary>
        [SetUp]
        public virtual void Initialize()
        {
            Init();
        }

        /// <summary>
        /// Returns a HiveManager using Examine
        /// </summary>
        /// <returns></returns>
        protected override IHiveManager CreateHiveManager()
        {
            return FakeHiveCmsManager.NewWithExamine(new[] { FakeHiveCmsManager.CreateFakeTemplateMappingGroup(FrameworkContext) }, FrameworkContext);
        }

        ///// <summary>
        ///// Helper method to create a brand new test EntitySchema with the property editor passed in for each schema definition
        ///// </summary>
        ///// <param name="propEditor"></param>
        ///// <param name="name"></param>
        ///// <param name="alias"></param>
        ///// <param name="beforeCommit"></param>
        ///// <returns></returns>
        //protected EntitySchema CreateNewEntitySchema(dynamic propEditor, string name, string alias, Action<EntitySchema> beforeCommit)
        //{
        //    var docType = DevDataset.DocTypes.First();

        //    docType.Name = name;
        //    docType.Alias = alias;

        //    var schema = RebelCmsApplicationContext.FrameworkContext.TypeMappers.Map<DocumentTypeEditorModel, EntitySchema>(docType);
        //    foreach (var a in schema.AttributeDefinitions.Where(x => !x.Alias.StartsWith("system-")))
        //    {
        //        a.AttributeType.RenderTypeProvider = propEditor.Id.ToString();
        //    }
            
        //    //next commit the new doc type
        //    using (var writer = RebelCmsApplicationContext.Hive.OpenWriter<IContentStore>())
        //    {
        //        if (beforeCommit != null)
        //        {
        //            beforeCommit(schema);
        //        }
        //        writer.Repositories.Schemas.AddOrUpdate(schema);
        //        writer.Complete();
        //    }

        //    return schema;

        //}

        /// <summary>
        /// Puts all of the data types into the repo from CoreCmsData
        /// </summary>
        protected void AddRequiredDataToRepository()
        {
            // Create in-built attribute type definitions
            using (var writer = RebelCmsApplicationContext.Hive.OpenWriter<IContentStore>())
            {
                writer.Repositories.Schemas.AddOrUpdate(CoreCmsData.RequiredCoreSystemAttributeTypes());
                writer.Complete();
            }
            
            //create user attribute type defs
            using (var writer = RebelCmsApplicationContext.Hive.OpenWriter<IContentStore>())
            {
                writer.Repositories.Schemas.AddOrUpdate(CoreCmsData.RequiredCoreUserAttributeTypes());
                writer.Complete();
            }

            //create the core schemas
            using (var writer = RebelCmsApplicationContext.Hive.OpenWriter<IContentStore>())
            {
                writer.Repositories.Schemas.AddOrUpdate(CoreCmsData.RequiredCoreSchemas());
                writer.Complete();
            }

            //create the core root nodes
            using (var writer = RebelCmsApplicationContext.Hive.OpenWriter<IContentStore>())
            {
                foreach (var e in CoreCmsData.RequiredCoreRootNodes())
                {
                    writer.Repositories.AddOrUpdate(e);                    
                }
                writer.Complete();
            }
        }

        /// <summary>
        /// helper method to create a content entity and assigns the same property editor to each of the attributes
        /// </summary>
        /// <param name="propEditor"></param>
        /// <returns></returns>
        protected Revision<TypedEntity> CreateEntityRevision(PropertyEditor propEditor)
        {
            return CreateEntityRevision(propEditor, null);
        }

        protected Revision<TypedEntity> CreateEntityRevision(PropertyEditor propEditor, Action<Revision<TypedEntity>> beforeCommit)
        {
            EntitySchema docTypeEntity = CreateNewSchema(propEditor);

            //create some content properties including node name
            var nodeName = HiveModelCreationHelper.CreateAttribute(docTypeEntity.AttributeDefinitions.Single(x => x.Alias == NodeNameAttributeDefinition.AliasValue), "");
            var bodyText = HiveModelCreationHelper.CreateAttribute(docTypeEntity.AttributeDefinitions.Single(x => x.Alias == "bodyText"), "my-test-value1");
            var siteName = HiveModelCreationHelper.CreateAttribute(docTypeEntity.AttributeDefinitions.Single(x => x.Alias == "siteName"), "my-test-value2");

            var contentEntity = HiveModelCreationHelper.CreateVersionedTypedEntity(docTypeEntity, new[] { nodeName, bodyText, siteName });
            
            contentEntity.MetaData = new RevisionData(FixedStatusTypes.Draft);

            if (beforeCommit != null)
                beforeCommit(contentEntity);

            RebelCmsApplicationContext.AddPersistenceData(contentEntity);

            return contentEntity;
        }

        protected EntitySchema CreateNewSchema(PropertyEditor propEditor = null, string alias = "test")
        {
            if (propEditor == null)
                propEditor = new MandatoryPropertyEditor();

            var attributeType = HiveModelCreationHelper.CreateAttributeType("test", "test", "test");
            attributeType.RenderTypeProvider = propEditor.Id.ToString();
            RebelCmsApplicationContext.AddPersistenceData(attributeType);

            var schema = HiveModelCreationHelper.MockEntitySchema(alias, alias);
            
            schema.TryAddAttributeDefinition(HiveModelCreationHelper.CreateAttributeDefinition("siteName", "Site Name", "", attributeType, null));
            schema.TryAddAttributeDefinition(HiveModelCreationHelper.CreateAttributeDefinition("bodyText", "Body Text", "", attributeType, null));                                                                  
 
            //set all attribute defs to the attribute type
            schema.AttributeDefinitions.Where(x => !x.Alias.StartsWith("system-")).ForEach(x => x.AttributeType = attributeType);

            schema.RelationProxies.EnlistParent(new ContentRootSchema(), FixedRelationTypes.DefaultRelationType, 0);

            RebelCmsApplicationContext.AddPersistenceData(schema);

            return schema;
        }

    }
}
