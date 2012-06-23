using System;
using RebelCms.Framework;

namespace RebelCms.Hive
{
    public interface ICoreRepository<in TBaseEntity>
        : IDisposable, ICoreRelationsRepository, ICoreReadonlyRepository<TBaseEntity> 
        where TBaseEntity : class, IReferenceByHiveId
    {
        void AddOrUpdate(TBaseEntity entity);
        void Delete<T>(HiveId id) where T : TBaseEntity;
    }
}