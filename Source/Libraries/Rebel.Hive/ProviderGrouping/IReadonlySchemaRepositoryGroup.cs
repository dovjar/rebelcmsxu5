using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Hive.ProviderGrouping
{
    public interface IReadonlySchemaRepositoryGroup<out TFilter> : ICoreReadonlyRepository<AbstractSchemaPart>
        where TFilter : class, IProviderTypeFilter
    {
        IReadonlyRevisionRepositoryGroup<TFilter, EntitySchema> Revisions { get; }
    }
}