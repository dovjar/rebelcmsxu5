namespace Rebel.Cms.Web.FluentExtensions
{
    using Rebel.Cms.Web.Model;
    using Rebel.Framework.Persistence.Model;
    using Rebel.Framework.Persistence.Model.Versioning;
    using Rebel.Hive;
    using Rebel.Hive.RepositoryTypes;
    using global::System;

    public class CmsBuilderStep<TProviderFilter> : BuilderStarter<TProviderFilter>, ICmsBuilderStep<TProviderFilter>
        where TProviderFilter : class, IProviderTypeFilter
    {
        public CmsBuilderStep(IHiveManager hiveManager)
            : base(hiveManager)
        {
        }
    }

    public interface IContentRevisionSaveResult<T> : IRevisionSaveResult<T>
        where T : TypedEntity
    {
        Content Content { get; }
    }

    public class ContentRevisionSaveResult<T> : RevisionSaveResult<T>, IContentRevisionSaveResult<T>
        where T : TypedEntity
    {
        public ContentRevisionSaveResult(bool success, IRevision<T> item, params Exception[] errors)
            : base(success, item, errors)
        {
        }

        #region Implementation of IContentRevisionSaveResult<T>

        public Content Content
        {
            get
            {
                return new Content(Item);
            }
        }

        #endregion
    }

    public interface IContentRevisionBuilderStep<T, out TProviderFilter> : ICmsBuilderStep<TProviderFilter>, IRevisionBuilderStep<TypedEntity, TProviderFilter>
        where TProviderFilter : class, IProviderTypeFilter
        where T : TypedEntity
    {
        Content Content { get; }
        bool RelateToRoot { get; set; }
    }

    public class ContentRevisionBuilderStep<T, TProviderFilter> : CmsBuilderStep<TProviderFilter>, IContentRevisionBuilderStep<T, TProviderFilter>
        where TProviderFilter : class, IProviderTypeFilter
        where T : TypedEntity // TODO: we use TypedEntity rather than Content due to covariance problems with our mapping of Revision<T>, needs converting fully to IRevision<T> and the mappers need updating to support it
    {
        public ContentRevisionBuilderStep(IHiveManager hiveManager, Revision<TypedEntity> revision)
            : base(hiveManager)
        {
            Revision = revision;
            RelateToRoot = false;
        }

        public ContentRevisionBuilderStep(IHiveManager hiveManager, T content)
            : base(hiveManager)
        {
            Revision = new Revision<TypedEntity>(content);
            _contentInstance = content as Content;
            RelateToRoot = false;
        }

        #region Implementation of IContentBuilderStep<T,out TProviderFilter>

        private Revision<TypedEntity> _revision;
        /// <summary>
        /// Gets or sets the revision being built.
        /// </summary>
        /// <value>
        /// The revision.
        /// </value>
        public Revision<TypedEntity> Revision
        {
            get
            {
                return _revision;
            }
            set
            {
                if (_revision == value) return;
                _revision = value;
                _contentInstance = null;
            }
        }

        /// <summary>
        /// Gets the current item being built.
        /// </summary>
        public TypedEntity Item
        {
            get
            {
                return Revision.Item;
            }
        }

        private Content _contentInstance;
        /// <summary>
        /// Gets the content instance from the current <see cref="Revision"/> being built.
        /// </summary>
        public Content Content
        {
            get
            {
                return _contentInstance ?? (_contentInstance = Revision == null ? null : new Content(Revision.Item));
            }
        }

        public bool RelateToRoot { get; set; }

        #endregion
    }
}