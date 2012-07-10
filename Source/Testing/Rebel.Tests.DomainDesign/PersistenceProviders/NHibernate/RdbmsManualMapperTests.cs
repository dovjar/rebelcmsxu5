using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rebel.Framework;
using Rebel.Framework.Context;
using Rebel.Framework.Persistence.DataManagement;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.ProviderSupport;
using Rebel.Framework.Persistence.RdbmsModel;
using Rebel.Framework.Persistence.RdbmsModel.Mapping;
using Rebel.Framework.ProviderSupport;
using Rebel.Framework.TypeMapping;
using Rebel.Tests.Extensions;

namespace Rebel.Tests.DomainDesign.PersistenceProviders.NHibernate
{
    [TestClass]
    public class RdbmsManualMapperTests
    {
     
        private class FakeLookupHelper : AbstractLookupHelper
        {
            public override T Lookup<T>(HiveId id)
            {
                return null;
            }
            public override void CacheCreation<T>(T item)
            {
                return;
            }
        }

        private class FakeHiveProvider : IHiveProvider
        {
            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public IFrameworkContext FrameworkContext
            {
                get { throw new NotImplementedException(); }
            }

            public AbstractDataContextFactory DataContextFactory
            {
                get { throw new NotImplementedException(); }
            }

            public string ProviderKey
            {
                get { throw new NotImplementedException(); }
            }

            public string ProviderAlias
            {
                get { return "fake-provider"; }
            }

            public AbstractProviderBootstrapper Bootstrapper
            {
                get { throw new NotImplementedException(); }
            }

            public int PriorityOrdinal
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }
        }

        [TestMethod]
        public void From_EntitySchema_With_Relations_To_AttributeSchemaDefinition()
        {
            //Arrange

            var mapper = new ManualMapper(new FakeLookupHelper(), new FakeHiveProvider());
            mapper.Configure();

            var entityParent = HiveModelCreationHelper.MockEntitySchema("test-schema-parent", "parent");
            entityParent.Id = HiveId.ConvertIntToGuid(1);
            var entityChild = HiveModelCreationHelper.MockEntitySchema("test-schema-child", "child");
            entityChild.Id = HiveId.ConvertIntToGuid(2);
            
            entityParent.Relations.Add(FixedRelationTypes.SchemaTreeRelationType, entityChild);

            //Act

            var resultParent = mapper.Map<EntitySchema, AttributeSchemaDefinition>(entityParent);
            //var resultChild = mapper.Map<EntitySchema, AttributeSchemaDefinition>(entityChild);

            //Assert

            Assert.AreEqual(entityParent.Alias, resultParent.Alias);
            Assert.AreEqual(entityParent.Name.ToString(), resultParent.Name);
            Assert.AreEqual(entityParent.AttributeDefinitions.Count, resultParent.AttributeDefinitions.Count);
            Assert.AreEqual(entityParent.AttributeTypes.Count, resultParent.AttributeDefinitions.Select(x => x.AttributeType).DistinctBy(x => x.Alias).Count());
            Assert.AreEqual(entityParent.Relations.Count(), resultParent.OutgoingRelations.Count);
            Assert.AreEqual(entityParent.Relations.Single().Source.Id, resultParent.OutgoingRelations.First().StartNode.Id);
            Assert.AreEqual(entityParent.Relations.Single().Destination.Id, resultParent.OutgoingRelations.First().EndNode.Id);

            //BUG: If you call entityChild.Relations.Count() an infinite loop occurs :(
            //Assert.AreEqual(entityChild.Relations.Count(), resultChild.OutgoingRelations.Count);

        }

        [TestMethod]
        public void From_AttributeSchemaDefinition_With_Relations_To_EntitySchema()
        {
            //Arrange

            var mapper = new ManualMapper(new FakeLookupHelper(), new FakeHiveProvider());
            mapper.Configure();

            var entityParent = new AttributeSchemaDefinition() { Id = Guid.NewGuid(),  Alias = "test-parent"};
            var entityChild = new AttributeSchemaDefinition() { Id = Guid.NewGuid(), Alias = "test-child" };

            entityParent.OutgoingRelations.Add(new NodeRelation()
            {
                StartNode = entityParent,
                EndNode = entityChild,
                NodeRelationType = new NodeRelationType()
                {
                    Alias = FixedRelationTypes.SchemaTreeRelationType.RelationName,
                    Id = FixedRelationTypes.SchemaTreeRelationType.RelationName.EncodeAsGuid()
                }
            });

            //Act

            var resultParent = mapper.Map<AttributeSchemaDefinition, EntitySchema>(entityParent);
            //var resultChild = mapper.Map<TypedEntity, NodeVersion>(entityChild);

            //Assert

            Assert.AreEqual(entityParent.Alias, resultParent.Alias);
            Assert.AreEqual(entityParent.OutgoingRelations.Count(), resultParent.Relations.Count());
            Assert.AreEqual(entityParent.OutgoingRelations.Single().StartNode.Id, (Guid)resultParent.Relations.Single().Source.Id.Value);
            Assert.AreEqual(entityParent.OutgoingRelations.Single().EndNode.Id, (Guid)resultParent.Relations.Single().Destination.Id.Value);

            //BUG: If you call entityChild.Relations.Count() an infinite loop occurs :(
            //Assert.AreEqual(entityChild.Relations.Count(), resultChild.Node.IncomingRelations.Count);

        }

        [TestMethod]
        public void From_TypedEntity_With_Relations_To_NodeVersion()
        {
            //Arrange

            var mapper = new ManualMapper(new FakeLookupHelper(), new FakeHiveProvider());
            mapper.Configure();

            var entityParent = HiveModelCreationHelper.MockTypedEntity(false);
            entityParent.Id = HiveId.ConvertIntToGuid(1);
            var entityChild = HiveModelCreationHelper.MockTypedEntity(false);
            entityChild.Id = HiveId.ConvertIntToGuid(2);

            entityParent.Relations.Add(FixedRelationTypes.ContentTreeRelationType, entityChild);

            //Act

            var resultParent = mapper.Map<TypedEntity, NodeVersion>(entityParent);
            //var resultChild = mapper.Map<TypedEntity, NodeVersion>(entityChild);

            //Assert

            Assert.AreEqual(entityParent.EntitySchema.Alias, resultParent.AttributeSchemaDefinition.Alias);
            Assert.AreEqual(entityParent.Attributes.Count, resultParent.Attributes.Count);            
            Assert.AreEqual(entityParent.Relations.Count(), resultParent.Node.OutgoingRelations.Count);
            Assert.AreEqual(entityParent.Relations.Single().Source.Id, resultParent.Node.OutgoingRelations.First().StartNode.Id);
            Assert.AreEqual(entityParent.Relations.Single().Destination.Id, resultParent.Node.OutgoingRelations.First().EndNode.Id);

            //BUG: If you call entityChild.Relations.Count() an infinite loop occurs :(
            //Assert.AreEqual(entityChild.Relations.Count(), resultChild.Node.IncomingRelations.Count);

        }

        [TestMethod]
        public void From_NodeVersion_With_Relations_To_TypedEntity()
        {
            //Arrange

            var mapper = new ManualMapper(new FakeLookupHelper(), new FakeHiveProvider());
            mapper.Configure();

            var entityParent = new NodeVersion()
                {
                    Id = Guid.NewGuid(),             
                    Node = new Node{ Id = Guid.NewGuid() },
                    AttributeSchemaDefinition = new AttributeSchemaDefinition() {Alias = "test-parent"},
                    NodeVersionStatuses = new[]{new NodeVersionStatusHistory() { Date = DateTime.Now, Id = Guid.NewGuid(), NodeVersionStatusType  = new NodeVersionStatusType() }, }
                };
            entityParent.Node.NodeVersions = new[] {entityParent};
            var entityChild = new NodeVersion()
            {
                Id = Guid.NewGuid(),
                Node = new Node { Id = Guid.NewGuid() },
                AttributeSchemaDefinition = new AttributeSchemaDefinition() { Alias = "test-child" },
                NodeVersionStatuses = new[] { new NodeVersionStatusHistory() { Date = DateTime.Now, Id = Guid.NewGuid(), NodeVersionStatusType = new NodeVersionStatusType() }, }
            };
            entityChild.Node.NodeVersions = new[] {entityChild};

            entityParent.Node.OutgoingRelations.Add(new NodeRelation()
                {
                    StartNode = entityParent.Node,
                    EndNode = entityChild.Node,
                    NodeRelationType = new NodeRelationType()
                        {
                            Alias = FixedRelationTypes.ContentTreeRelationType.RelationName,
                            Id = FixedRelationTypes.ContentTreeRelationType.RelationName.EncodeAsGuid()
                        }
                });

            //Act

            var resultParent = mapper.Map<NodeVersion, TypedEntity>(entityParent);
            //var resultChild = mapper.Map<TypedEntity, NodeVersion>(entityChild);

            //Assert

            Assert.AreEqual(entityParent.AttributeSchemaDefinition.Alias, resultParent.EntitySchema.Alias);
            Assert.AreEqual(entityParent.Attributes.Count, resultParent.Attributes.Count);
            Assert.AreEqual(entityParent.Node.OutgoingRelations.Count(), resultParent.Relations.Count());
            Assert.AreEqual(entityParent.Node.OutgoingRelations.Single().StartNode.Id, (Guid)resultParent.Relations.Single().Source.Id.Value);
            Assert.AreEqual(entityParent.Node.OutgoingRelations.Single().EndNode.Id, (Guid)resultParent.Relations.Single().Destination.Id.Value);

            //BUG: If you call entityChild.Relations.Count() an infinite loop occurs :(
            //Assert.AreEqual(entityChild.Relations.Count(), resultChild.Node.IncomingRelations.Count);

        }

       

    }
}