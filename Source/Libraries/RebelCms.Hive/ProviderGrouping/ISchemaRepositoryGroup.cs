using RebelCms.Framework.Persistence.Model;
using RebelCms.Framework.Persistence.Model.Attribution.MetaData;
using RebelCms.Hive.RepositoryTypes;

namespace RebelCms.Hive.ProviderGrouping
{
    public interface ISchemaRepositoryGroup<out TFilter> : ICoreRepository<AbstractSchemaPart>
        where TFilter : class, IProviderTypeFilter
    {
        /// <summary>
        /// Used to access providers that can get or set revisions for <see cref="AbstractSchemaPart"/> types.
        /// </summary>
        /// <value>The revisions.</value>
        IRevisionRepositoryGroup<TFilter, EntitySchema> Revisions { get; }
    }
}