namespace Umbraco.Cms.Web
{
    #region Imports

    using Umbraco.Cms.Web.Model;
    using Umbraco.Framework;
    using Umbraco.Hive;
    using Umbraco.Hive.RepositoryTypes;
    using global::System.Linq;

    #endregion

    public static class CmsHiveManagerExtensions
    {
        /// <summary>
        /// Creates an <see cref="IRenderViewHiveManagerWrapper"/> from an <see cref="IHiveManager"/>.
        /// </summary>
        /// <param name="hiveManager">The hive manager.</param>
        /// <returns></returns>
        public static IRenderViewHiveManagerWrapper Cms(this IHiveManager hiveManager)
        {
            return new RenderViewHiveManagerWrapper(hiveManager);
        }

        /// <summary>
        /// Opens a reader, and starts a query of the specified hive manager.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="hiveManager">The hive manager.</param>
        /// <returns></returns>
        public static IQueryable<TResult> Query<TResult, TProviderFilter>(this IHiveManager hiveManager)
            where TResult : class, IReferenceByHiveId
            where TProviderFilter : class, IProviderTypeFilter
        {
            using (var uow = hiveManager.OpenReader<TProviderFilter>())
            {
                return uow.Repositories.Select(x => hiveManager.FrameworkContext.TypeMappers.Map<TResult>(x));
            }
        }

        /// <summary>
        /// A shortcut for building queries limited to media, specifically Hive providers matching the groups specified by <see cref="IMediaStore"/> (typically <value>media://</value>).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hiveManager">The hive manager.</param>
        /// <returns></returns>
        public static IQueryable<T> QueryMedia<T>(this IHiveManager hiveManager)
            where T : class, IReferenceByHiveId
        {
            return hiveManager.Query<T, IMediaStore>();
        }

        /// <summary>
        /// A shortcut for building queries limited to media, specifically Hive providers matching the groups specified by <see cref="IMediaStore"/> (typically <value>media://</value>).
        /// </summary>
        /// <param name="hiveManager">The hive manager.</param>
        /// <returns></returns>
        public static IQueryable<Media> QueryMedia(this IHiveManager hiveManager)
        {
            return QueryMedia<Media>(hiveManager);
        }

        /// <summary>
        /// A shortcut for building queries limited to content, specifically Hive providers matching the groups specified by <see cref="IContentStore"/> (typically <value>content://</value>).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hiveManager">The hive manager.</param>
        /// <returns></returns>
        public static IQueryable<T> QueryContent<T>(this IHiveManager hiveManager)
            where T : class, IReferenceByHiveId
        {
            return hiveManager.Query<T, IContentStore>();
        }

        /// <summary>
        /// A shortcut for building queries limited to content, specifically Hive providers matching the groups specified by <see cref="IContentStore"/> (typically <value>content://</value>).
        /// </summary>
        /// <param name="hiveManager">The hive manager.</param>
        /// <returns></returns>
        public static IQueryable<Content> QueryContent(this IHiveManager hiveManager)
        {
            return QueryContent<Content>(hiveManager);
        }

        /// <summary>
        /// Gets an item by Id, or <value>null</value> if none is found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static T GetById<T>(this IQueryable<T> source, HiveId id)
            where T : class, IReferenceByHiveId
        {
            return source.FirstOrDefault(x => x.Id == id);
        }

        /// <summary>
        /// Gets an item by Id, or <value>null</value> if none is found or if <paramref name="idAsString"/> cannot be parsed as a <see cref="HiveId"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="idAsString">The id as a string.</param>
        /// <returns></returns>
        public static T GetById<T>(this IQueryable<T> source, string idAsString)
            where T : class, IReferenceByHiveId
        {
            var parsed = HiveId.TryParse(idAsString);
            if (!parsed.Success) return null;
            return source.FirstOrDefault(x => x.Id == parsed.Result);
        }
    }
}