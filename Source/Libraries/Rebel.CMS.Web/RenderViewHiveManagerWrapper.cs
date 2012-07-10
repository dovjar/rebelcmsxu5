using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Cms.Web.Model;
using Rebel.Framework.Context;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Hive;
using Rebel.Hive.Configuration;
using Rebel.Hive.Diagnostics;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Cms.Web
{
    using Rebel.Framework;
    using Rebel.Framework.Persistence.Model.Constants;
    using Rebel.Hive.ProviderSupport;

    public interface IRenderViewHiveManagerWrapper : IHiveManager
    {
        IQueryable<Content> Content { get; }
    }

    public class RenderViewHiveManagerWrapper : DisposableObject, IRenderViewHiveManagerWrapper
    {
        private readonly IHiveManager _innerHiveManager;

        public RenderViewHiveManagerWrapper(IHiveManager manager)
        {
            _innerHiveManager = manager;
        }

        /// <summary>
        /// Starts a query for content excluding items in the backoffice recycle bin
        /// </summary>
        public IQueryable<Content> Content
        {
            get
            {
                using (var uow = this.OpenReader<IContentStore>())
                {
                    var descendantsOfRecycleBin =
                        uow.Repositories
                            .GetDescendentRelations(FixedHiveIds.ContentRecylceBin)
                            .Select(x => x.DestinationId).ToArray();
                    CheckRecycleBinTooLarge(descendantsOfRecycleBin);
                    return uow.Repositories.Query<Content>().ExcludeIds(descendantsOfRecycleBin).OfRevisionType(FixedStatusTypes.Published);
                }
            }
        }

        private static void CheckRecycleBinTooLarge(HiveId[] descendantsOfRecycleBin)
        {
            // Sanity-check on recycle bin item count before running query
            if (descendantsOfRecycleBin.Length > 1000)
            {
                throw new InvalidOperationException(
                    "There are more than 1000 items (including descendants) in the Recycle Bin. Queries that don't specify a parent can't run unless there are fewer than 1000 items in the recycle bin; please consider emptying it in the Rebel backoffice");
            }
        }

        /// <summary>
        /// Starts a query for media excluding items in the backoffice recycle bin
        /// </summary>
        public IQueryable<Media> Media
        {
            get
            {
                using (var uow = this.OpenReader<IMediaStore>())
                {
                    var descendantsOfRecycleBin =
                        uow.Repositories
                            .GetDescendentRelations(FixedHiveIds.ContentRecylceBin)
                            .Select(x => x.DestinationId).ToArray();
                    CheckRecycleBinTooLarge(descendantsOfRecycleBin);
                    return uow.Repositories.Query<Media>().ExcludeIds(descendantsOfRecycleBin).OfRevisionType(FixedStatusTypes.Published);
                }
            }
        }

        #region Wrapped methods from inner manager
        /// <summary>
        /// Gets the perf counter manager.
        /// </summary>
        /// <value>The perf counter manager.</value>
        public HiveCounterManager PerfCounterManager
        {
            get { return _innerHiveManager.PerfCounterManager; }
        }

        /// <summary>
        /// Gets the provider groups managed by this <see cref="IHiveManager"/>.
        /// </summary>
        /// <value>The provider groups.</value>
        public IEnumerable<ProviderMappingGroup> ProviderGroups
        {
            get { return _innerHiveManager.ProviderGroups; }
        }

        /// <summary>
        /// Gets the framework context.
        /// </summary>
        /// <value>The framework context.</value>
        public IFrameworkContext FrameworkContext
        {
            get { return _innerHiveManager.FrameworkContext; }
        }

        /// <summary>
        /// Gets the manager instance id.
        /// </summary>
        /// <value>The manager id.</value>
        public Guid ManagerId
        {
            get { return _innerHiveManager.ManagerId; }
        }

        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <value>The context.</value>
        public RepositoryContext Context
        {
            get
            {
                return _innerHiveManager.Context;
            }
        }

        /// <summary>
        /// Gets all the read providers registered with this manager.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public IEnumerable<ReadonlyProviderSetup> GetAllReadProviders()
        {
            return _innerHiveManager.GetAllReadProviders();
        }

        /// <summary>
        /// Gets all read write providers registered with this manager.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public IEnumerable<ProviderSetup> GetAllReadWriteProviders()
        {
            return _innerHiveManager.GetAllReadWriteProviders();
        }

        /// <summary>
        /// Gets a <see cref="ReadonlyGroupUnitFactory{TFilter}"/> for a group of providers, by finding a matching group in the <see cref="IHiveManager.ProviderGroups"/> collection where the
        /// provider's mapping group root Uri matches the value with which <typeparamref name="TFilter"/> has been decorated. <typeparamref name="TFilter"/> must be decorated
        /// with a <see cref="RepositoryTypeAttribute"/> containing the provider group root Uri.
        /// </summary>
        /// <typeparam name="TFilter">The <see cref="IProviderTypeFilter"/> used to create extension methods against this provider group.</typeparam>
        /// <returns></returns>
        public ReadonlyGroupUnitFactory<TFilter> GetReader<TFilter>() where TFilter : class, IProviderTypeFilter
        {
            return _innerHiveManager.GetReader<TFilter>();
        }

        /// <summary>
        /// Gets a <see cref="GroupUnitFactory{TFilter}"/> for a group of providers, by finding a matching group in the <see cref="IHiveManager.ProviderGroups"/> collection where the
        /// provider's mapping group root Uri matches the value with which <typeparamref name="TFilter"/> has been decorated. <typeparamref name="TFilter"/> must be decorated
        /// with a <see cref="RepositoryTypeAttribute"/> containing the provider group root Uri.
        /// </summary>
        /// <typeparam name="TFilter">The <see cref="IProviderTypeFilter"/> used to create extension methods against this provider group.</typeparam>
        /// <returns></returns>
        public GroupUnitFactory<TFilter> GetWriter<TFilter>() where TFilter : class, IProviderTypeFilter
        {
            return _innerHiveManager.GetWriter<TFilter>();
        }

        /// <summary>
        /// Gets a <see cref="ReadonlyGroupUnitFactory{TFilter}"/> for a group of providers, by finding a matching group in the <see cref="IHiveManager.ProviderGroups"/> collection where the
        /// provider's mapping group root Uri matches <paramref name="providerMappingRoot"/>. <typeparamref name="TFilter"/> is ignored when searching for a matching provider group,
        /// but can still be used to assign specific extension methods to this provider group request.
        /// </summary>
        /// <typeparam name="TFilter">The type of the filter.</typeparam>
        /// <param name="providerMappingRoot">The provider mapping root.</param>
        /// <returns></returns>
        public ReadonlyGroupUnitFactory<TFilter> GetReader<TFilter>(Uri providerMappingRoot) where TFilter : class, IProviderTypeFilter
        {
            return _innerHiveManager.GetReader<TFilter>(providerMappingRoot);
        }

        /// <summary>
        /// Gets a <see cref="ReadonlyGroupUnitFactory"/> for a group of providers, by finding a matching group in the <see cref="IHiveManager.ProviderGroups"/> collection where the
        /// provider's mapping group root Uri matches <paramref name="providerMappingRoot"/>.
        /// </summary>
        /// <param name="providerMappingRoot">The provider mapping root.</param>
        /// <returns></returns>
        public ReadonlyGroupUnitFactory GetReader(Uri providerMappingRoot)
        {
            return _innerHiveManager.GetReader(providerMappingRoot);
        }

        /// <summary>
        /// Gets a <see cref="GroupUnitFactory{TFilter}"/> for a group of providers, by finding a matching group in the <see cref="IHiveManager.ProviderGroups"/> collection where the
        /// provider's mapping group root Uri matches <paramref name="providerMappingRoot"/>. <typeparamref name="TFilter"/> is ignored when searching for a matching provider group,
        /// but can still be used to assign specific extension methods to this provider group request.
        /// </summary>
        /// <typeparam name="TFilter">The type of the filter.</typeparam>
        /// <param name="providerMappingRoot">The provider mapping root.</param>
        /// <returns></returns>
        public GroupUnitFactory<TFilter> GetWriter<TFilter>(Uri providerMappingRoot) where TFilter : class, IProviderTypeFilter
        {
            return _innerHiveManager.GetWriter<TFilter>(providerMappingRoot);
        }

        /// <summary>
        /// Gets a <see cref="GroupUnitFactory"/> for a group of providers, by finding a matching group in the <see cref="IHiveManager.ProviderGroups"/> collection where the
        /// provider's mapping group root Uri matches <paramref name="providerMappingRoot"/>.
        /// </summary>
        /// <param name="providerMappingRoot">The provider mapping root.</param>
        /// <returns></returns>
        public GroupUnitFactory GetWriter(Uri providerMappingRoot)
        {
            return _innerHiveManager.GetWriter(providerMappingRoot);
        }

        #endregion

        #region Overrides of DisposableObject

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            _innerHiveManager.IfNotNull(x => x.Dispose());
        }

        #endregion
    }
}
