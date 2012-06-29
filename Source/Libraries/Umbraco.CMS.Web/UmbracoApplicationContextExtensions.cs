namespace Umbraco.Cms.Web
{
    using Umbraco.Cms.Web.Context;
    using Umbraco.Framework;
    using Umbraco.Framework.ProviderSupport;
    using global::System;
    using global::System.Linq;
    using global::System.Web.Caching;

    public static class UmbracoApplicationContextExtensions
    {
        #region Public Methods

        /// <summary>
        ///   Returns true if all providers have a Completed installation status
        /// </summary>
        /// <param name="appContext"> </param>
        /// <returns> </returns>
        public static bool AllProvidersInstalled(this IUmbracoApplicationContext appContext)
        {
            return appContext.FrameworkContext.ApplicationCache.GetOrCreate(
                "all-providers-installed",
                () =>
                    {
                        var isInstalled = appContext.GetInstallStatus().All(status => status.StatusType == InstallStatusType.Completed);

                        // If not all providers are installed, basically don't cache it
                        if (isInstalled)
                        {
                            return new HttpRuntimeCacheParameters<bool>(true) { CacheItemPriority = CacheItemPriority.NotRemovable, AbsoluteExpiration = DateTime.MaxValue };
                        }
                        else
                        {
                            return new HttpRuntimeCacheParameters<bool>(false) { SlidingExpiration = TimeSpan.FromMilliseconds(1) };
                        }
                    });
        }

        public static bool AnyProvidersHaveStatus(this IUmbracoApplicationContext appContext, InstallStatusType status)
        {
            return appContext.GetInstallStatus().Any(s => s.StatusType == status);
        }

        #endregion
    }
}