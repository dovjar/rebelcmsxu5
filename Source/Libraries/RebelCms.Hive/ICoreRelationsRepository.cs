using RebelCms.Framework.Persistence.Model;
using RebelCms.Framework.Persistence.Model.Associations;

namespace RebelCms.Hive
{
    public interface ICoreRelationsRepository 
        : ICoreReadonlyRelationsRepository
    {
        void AddRelation(IReadonlyRelation<IRelatableEntity, IRelatableEntity> item);
        void RemoveRelation(IRelationById item);
    }
}