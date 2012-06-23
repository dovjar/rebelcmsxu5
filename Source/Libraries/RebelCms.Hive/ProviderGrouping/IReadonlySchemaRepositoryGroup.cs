using RebelCms.Framework.Persistence.Model;
using RebelCms.Framework.Persistence.Model.Attribution.MetaData;
using RebelCms.Hive.RepositoryTypes;

namespace RebelCms.Hive.ProviderGrouping
{
    public interface IReadonlySchemaRepositoryGroup<out TFilter> : ICoreReadonlyRepository<AbstractSchemaPart>
        where TFilter : class, IProviderTypeFilter
    {
        IReadonlyRevisionRepositoryGroup<TFilter, EntitySchema> Revisions { get; }
    }
}