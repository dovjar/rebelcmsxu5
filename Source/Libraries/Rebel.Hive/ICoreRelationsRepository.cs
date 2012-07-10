using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Associations;

namespace Rebel.Hive
{
    public interface ICoreRelationsRepository 
        : ICoreReadonlyRelationsRepository
    {
        void AddRelation(IReadonlyRelation<IRelatableEntity, IRelatableEntity> item);
        void RemoveRelation(IRelationById item);
    }
}