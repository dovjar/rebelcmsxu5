using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Rebel.Framework.Configuration;
using Rebel.Framework.Localization;
using Rebel.Framework.Tasks;
using Rebel.Framework.TypeMapping;

namespace Rebel.Framework.Context
{
    using Rebel.Framework.Caching;
    using Rebel.Framework.Serialization;

    public class DefaultFrameworkCaches : DisposableObject, IFrameworkCaches
    {
        private readonly AbstractCacheProvider _limitedLifetime;

        private readonly AbstractCacheProvider _extendedLifetime;

        public DefaultFrameworkCaches(AbstractCacheProvider limitedLifetime, AbstractCacheProvider extendedLifetime)
        {
            Mandate.ParameterNotNull(limitedLifetime, "limitedLifetime");
            Mandate.ParameterNotNull(extendedLifetime, "extendedLifetime");

            _limitedLifetime = limitedLifetime;
            _extendedLifetime = extendedLifetime;
        }

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            _limitedLifetime.IfNotNull(x => x.Dispose());
            _extendedLifetime.IfNotNull(x => x.Dispose());
        }

        /// <summary>
        /// Gets the cache provider for entries that should have a limited lifetime, for example the lifetime of a web request.
        /// </summary>
        public AbstractCacheProvider LimitedLifetime
        {
            get { return _limitedLifetime; }
        }

        /// <summary>
        /// Gets the cache provider for entries that should have an extended lifetime, for example multiple web requests. Dependant upon the provider used, these could survive application recyces.
        /// </summary>
        public AbstractCacheProvider ExtendedLifetime
        {
            get { return _extendedLifetime; }
        }
    }

    /// <summary>
    /// The default implementation of <see cref="IFrameworkContext"/>
    /// </summary>
    /// <remarks></remarks>
    public class DefaultFrameworkContext : DisposableObject, IFrameworkContext
    {
        protected DefaultFrameworkContext(AbstractScopedCache scopedCache,
            AbstractApplicationCache applicationCache,
            AbstractFinalizer finalizer,
            IFrameworkCaches caches,
            AbstractSerializationService serialization)
        {
            ScopedCache = scopedCache;
            ApplicationCache = applicationCache;
            ScopedFinalizer = finalizer;
            Caches = caches;
            Serialization = serialization;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultFrameworkContext"/> class.
        /// </summary>
        /// <param name="textManager">The text manager.</param>
        /// <param name="typeMappers">The type mappers.</param>
        /// <param name="scopedCache">The scoped cache.</param>
        /// <param name="applicationCache">The application cache</param>
        /// <param name="finalizer">The finalizer.</param>
        /// <param name="taskMgr">The task manager.</param>
        /// <param name="caches">The lifetime-managed cache providers.</param>
        public DefaultFrameworkContext(
            TextManager textManager, 
            MappingEngineCollection typeMappers, 
            AbstractScopedCache scopedCache,
            AbstractApplicationCache applicationCache,
            AbstractFinalizer finalizer,
            ApplicationTaskManager taskMgr,
            IFrameworkCaches caches,
            AbstractSerializationService serialization)
            : this(scopedCache, applicationCache, finalizer, caches, serialization)
        {
            TextManager = textManager;
            TypeMappers = typeMappers;
            TaskManager = taskMgr;
        }

        private LanguageInfo _currentLanguage;

        /// <summary>
        /// Gets the task manager
        /// </summary>
        public ApplicationTaskManager TaskManager { get; private set; }

        /// <summary>
        /// Gets or sets the current language used for localization.
        /// </summary>
        /// <value>The current language.</value>
        /// <remarks></remarks>
        public LanguageInfo CurrentLanguage
        {
            get { return _currentLanguage; }
            set
            {                
                if (value == null)
                {
                    throw new ArgumentNullException(
                        TextManager.Get<DefaultFrameworkContext>("FrameworkContext.CurrentLanguage.NullException"));
                }
                _currentLanguage = value;
            }
        }

        /// <summary>
        /// Gets the text manager to be used for localization
        /// </summary>
        /// <value>The text manager.</value>
        /// <remarks></remarks>
        public TextManager TextManager { get; protected set; }

        /// <summary>
        /// Gets a collection of registered type mappers.
        /// </summary>
        /// <remarks></remarks>
        public MappingEngineCollection TypeMappers { get; protected set; }

        /// <summary>
        /// Gets a scoped finalizer queue.
        /// </summary>
        /// <remarks></remarks>
        public AbstractFinalizer ScopedFinalizer { get; protected set; }

        /// <summary>
        /// Gets the scoped cache bag
        /// </summary>
        public AbstractScopedCache ScopedCache { get; protected set; }

        /// <summary>
        /// Gets the application cache bag
        /// </summary>
        public AbstractApplicationCache ApplicationCache { get; protected set; }

        /// <summary>
        /// Gets the serialization service.
        /// </summary>
        public AbstractSerializationService Serialization { get; protected set; }

        /// <summary>
        /// Gets the lifetime-scoped cache providers for this context.
        /// </summary>
        public IFrameworkCaches Caches { get; protected set; }

        #region Overrides of DisposableObject

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            ScopedFinalizer.IfNotNull(x =>
                {
                    x.FinalizeScope();
                    x.Dispose();
                });
            ScopedCache.IfNotNull(x =>
                {
                    x.ScopeComplete();
                    x.Dispose();
                });
            Caches.IfNotNull(x => x.Dispose());
        }

        #endregion
    }
}
