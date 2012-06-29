using Umbraco.Framework.Configuration;
using Umbraco.Framework.Localization;
using Umbraco.Framework.Tasks;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Framework.Context
{
    using System;
    using Umbraco.Framework.Caching;
    using Umbraco.Framework.Serialization;

    /// <summary>
    /// An aggregatation of common Framework services and components.
    /// </summary>
    /// <remarks></remarks>
    public interface IFrameworkContext : IDisposable
    {
        /// <summary>
        /// Gets the task manager
        /// </summary>
        ApplicationTaskManager TaskManager { get;  }

        /// <summary>
        /// Gets or sets the current language used for localization.
        /// </summary>
        /// <value>
        /// The current language.
        /// </value>
        LanguageInfo CurrentLanguage { get; set; }

        /// <summary>
        /// Gets the text manager to be used for localization
        /// </summary>
        TextManager TextManager {get;}

        /// <summary>
        /// Gets a collection of registered type mappers.
        /// </summary>
        /// <remarks></remarks>
        MappingEngineCollection TypeMappers { get; }

        /// <summary>
        /// Gets a scoped finalizer queue.
        /// </summary>
        /// <remarks></remarks>
        AbstractFinalizer ScopedFinalizer { get; }

        /// <summary>
        /// Gets a scoped cache bag
        /// </summary>
        AbstractScopedCache ScopedCache { get;  }

        /// <summary>
        /// Gets the application cache bag
        /// </summary>
        AbstractApplicationCache ApplicationCache { get; }

        /// <summary>
        /// Gets the serialization service.
        /// </summary>
        AbstractSerializationService Serialization { get; }

        /// <summary>
        /// Gets the lifetime-scoped cache providers for this context.
        /// </summary>
        IFrameworkCaches Caches { get; }
    }

    public interface IFrameworkCaches : IDisposable
    {
        /// <summary>
        /// Gets the cache provider for entries that should have a limited lifetime, for example the lifetime of a web request.
        /// </summary>
        AbstractCacheProvider LimitedLifetime { get; }

        /// <summary>
        /// Gets the cache provider for entries that should have an extended lifetime, for example multiple web requests. Dependant upon the provider used, these could survive application recyces.
        /// </summary>
        AbstractCacheProvider ExtendedLifetime { get; }
    }
}