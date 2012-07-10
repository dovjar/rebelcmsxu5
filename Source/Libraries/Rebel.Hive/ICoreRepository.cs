using System;
using Rebel.Framework;

namespace Rebel.Hive
{
    public interface ICoreRepository<in TBaseEntity>
        : IDisposable, ICoreRelationsRepository, ICoreReadonlyRepository<TBaseEntity> 
        where TBaseEntity : class, IReferenceByHiveId
    {
        void AddOrUpdate(TBaseEntity entity);
        void Delete<T>(HiveId id) where T : TBaseEntity;
    }
}