using System;
using System.Collections.Generic;
using Rebel.Cms.Web.Configuration;
using Rebel.Cms.Web.Security;
using Rebel.Framework.Context;
using Rebel.Framework.ProviderSupport;
using Rebel.Framework.Security;
using Rebel.Hive;

namespace Rebel.Cms.Web.Context
{
    /// <summary>
    /// Encapsulates information specific to an Rebel application
    /// </summary>
    public interface IRebelApplicationContext : IDisposable
    {
        IEnumerable<InstallStatus> GetInstallStatus();

        /// <summary>
        /// Gets the framework context.
        /// </summary>
        /// <remarks></remarks>
        IFrameworkContext FrameworkContext { get; }

        bool IsFirstRun { get; set; }

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
        /// Gets the settings associated with this Rebel application.
        /// </summary>
        /// <value>The settings.</value>
        RebelSettings Settings { get; }

        /// <summary>
        /// Gets the security service.
        /// </summary>
        ISecurityService Security { get; }
    }
}