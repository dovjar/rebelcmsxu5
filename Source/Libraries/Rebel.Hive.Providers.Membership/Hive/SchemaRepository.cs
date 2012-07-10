//using System;
//using System.Collections.Generic;
//using Rebel.Framework;
//using Rebel.Framework.Context;
//using Rebel.Framework.Persistence.Model;
//using Rebel.Framework.Persistence.Model.Associations;
//using Rebel.Framework.Persistence.ProviderSupport._Revised;
//using Rebel.Hive.ProviderSupport;

//namespace Rebel.Hive.Providers.Membership.Hive
//{
//    public class SchemaRepository : AbstractSchemaRepository
//    {
        
//        public SchemaRepository(
//            ProviderMetadata providerMetadata,
//            IFrameworkContext frameworkContext)
//            : base(providerMetadata, frameworkContext)
//        {            
//        }

//        protected override void DisposeResources()
//        {
//            Revisions.Dispose();
//            Transaction.Dispose();
//        }

//        protected override IEnumerable<T> PerformGet<T>(bool allOrNothing, params HiveId[] ids)
//        {
//            throw new NotImplementedException();
//        }

//        public override IRelationById PerformFindRelation(HiveId sourceId, HiveId destinationId, RelationType relationType)
//        {
//            throw new NotImplementedException();
//        }

//        public override IEnumerable<TEntity> PerformGetAll<TEntity>()
//        {
//            throw new NotImplementedException();
//        }

//        public override bool CanReadRelations
//        {
//            get { return true; }
//        }

//        public override IEnumerable<IRelationById> PerformGetParentRelations(HiveId childId, RelationType relationType = null)
//        {
//            throw new NotImplementedException();
//        }

//        public override IEnumerable<IRelationById> PerformGetAncestorRelations(HiveId descendentId, RelationType relationType = null)
//        {
//            throw new NotImplementedException();
//        }

//        public override IEnumerable<IRelationById> PerformGetDescendentRelations(HiveId ancestorId, RelationType relationType = null)
//        {
//            throw new NotImplementedException();
//        }

//        public override IEnumerable<IRelationById> PerformGetChildRelations(HiveId parentId, RelationType relationType = null)
//        {
//            throw new NotImplementedException();
//        }

//        public override bool Exists<TEntity>(HiveId id)
//        {
//            throw new NotImplementedException();
//        }

//        public override void PerformAddOrUpdate(AbstractSchemaPart entity)
//        {
//            throw new NotImplementedException();
//        }

//        public override void Delete<T>(HiveId id)
//        {
//            throw new NotImplementedException();
//        }

//        public override bool CanWriteRelations
//        {
//            get { return true; }
//        }

//        public override void AddRelation(IReadonlyRelation<IRelatableEntity, IRelatableEntity> item)
//        {
//            throw new NotImplementedException();
//        }

//        public override void RemoveRelation(IRelationById item)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}