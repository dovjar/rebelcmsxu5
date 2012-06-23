using System;
using System.Collections.Generic;
using RebelCms.Cms.Web.Configuration;
using RebelCms.Cms.Web.Security;
using RebelCms.Framework.Context;
using RebelCms.Framework.ProviderSupport;
using RebelCms.Framework.Security;
using RebelCms.Hive;

namespace RebelCms.Cms.Web.Context
{
    /// <summary>
    /// Encapsulates information specific to an RebelCms application
    /// </summary>
    public interface IRebelCmsApplicationContext : IDisposable
    {
        IEnumerable<InstallStatus> GetInstallStatus();

        /// <summary>
        /// Gets the framework context.
        /// </summary>
        /// <remarks></remarks>
        IFrameworkContext FrameworkContext { get; }

        /// <summary>
        /// Gets the application id, useful for debugging or tracing.
        /// </summary>
        /// <value>The request id.</value>
        Guid ApplicationId { get; }

        /// <summary>
        /// Gets an instance of <see cref="HiveManager"/> for this application.
        /// </summary>
        /// <value>The hive.</value>
        IHiveManager Hive { get; }

        /// <summary>
        /// Gets the settings associated with this RebelCms application.
        /// </summary>
        /// <value>The settings.</value>
        RebelCmsSettings Settings { get; }

        /// <summary>
        /// Gets the security service.
        /// </summary>
        ISecurityService Security { get; }
    }
}