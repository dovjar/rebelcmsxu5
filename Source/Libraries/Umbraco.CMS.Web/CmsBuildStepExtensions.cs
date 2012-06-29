using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Framework.Security;
using Umbraco.Framework.Security.Model.Entities;

namespace Umbraco.Cms.Web
{
    using Umbraco.Cms.Web.FluentExtensions;
    using Umbraco.Cms.Web.Model.BackOffice.Editors;
    using Umbraco.Framework;
    using Umbraco.Framework.Persistence;
    using Umbraco.Framework.Persistence.Model;
    using Umbraco.Framework.Persistence.Model.Associations;
    using Umbraco.Framework.Persistence.Model.Attribution;
    using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
    using Umbraco.Framework.Persistence.Model.Constants;
    using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
    using Umbraco.Framework.Persistence.Model.IO;
    using Umbraco.Hive;
    using Umbraco.Hive.RepositoryTypes;
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Linq;
    using global::System.Web;
    using Umbraco.Hive.ProviderGrouping;

    public static class CmsBuildStepExtensions
    {
        /// <summary>
        /// Returns a new <see cref="CmsBuilderStep{TProviderFilter}"/> based on the <see cref="IHiveManager"/> in <paramref name="builderStep"/>, making Cms-specific 
        /// extension methods available for use.
        /// </summary>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="builderStep">The builder step.</param>
        /// <returns></returns>
        public static ICmsBuilderStep<TProviderFilter> Cms<TProviderFilter>(
            this IBuilderStep<TProviderFilter> builderStep)
            where TProviderFilter : class, IProviderTypeFilter
        {
            return new CmsBuilderStep<TProviderFilter>(builderStep.HiveManager);
        }

        /// <summary>
        /// Returns a new <see cref="CmsBuilderStep{TProviderFilter}"/> based on <paramref name="hiveManager"/>, making Cms-specific 
        /// extension methods available for use.
        /// </summary>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="hiveManager">The hive manager.</param>
        /// <returns></returns>
        public static ICmsBuilderStep<TProviderFilter> Cms<TProviderFilter>(this IHiveManager hiveManager)
            where TProviderFilter : class, IProviderTypeFilter
        {
            return new CmsBuilderStep<TProviderFilter>(hiveManager);
        }

        /// <summary>
        /// Returns a new <see cref="IFileStoreStep{File, ICmsUploadFileStore}"/> making filestore-specific extension methods
        /// available for use.
        /// </summary>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="builderStep">The builder step.</param>
        /// <returns></returns>
        public static IFileStoreStep<File, ICmsUploadFileStore> UploadFileStore<TProviderFilter>(
            this ICmsBuilderStep<TProviderFilter> builderStep)
            where TProviderFilter : class, IProviderTypeFilter
        {
            return builderStep.HiveManager.FileStore<ICmsUploadFileStore>();
        }

        /// <summary>
        /// Returns a new <see cref="IFileStoreStep{File, ICmsUploadFileStore}"/> making filestore-specific extension methods
        /// available for use.
        /// </summary>
        /// <param name="hiveManager">The hive manager.</param>
        /// <returns></returns>
        public static IFileStoreStep<File, ICmsUploadFileStore> UploadFileStore(
            this IRenderViewHiveManagerWrapper hiveManager)
        {
            return hiveManager.FileStore<ICmsUploadFileStore>();
        }

        /// <summary>
        /// Starts the building of a new entity revision inside a Hive provider group specified by <typeparamref name="TProviderFilter"/>. Note that changes are not committed to Hive automatically after this step; you must call <see cref="Commit{T, TProviderFilter}"/> or manually add items to a writable Hive unit of work.
        /// </summary>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="builderStep">The builder step.</param>
        /// <param name="name">The name.</param>
        /// <param name="urlAlias">The URL alias.</param>
        /// <param name="contentTypeAlias">The content type alias.</param>
        /// <param name="setRootAsParent">If set to <c>true</c>, automatically set the content root as its parent.</param>
        /// <returns></returns>
        public static IContentRevisionBuilderStep<TypedEntity, TProviderFilter> NewRevision<TProviderFilter>(
            this ICmsBuilderStep<TProviderFilter> builderStep,
            string name,
            string urlAlias,
            string contentTypeAlias,
            bool setRootAsParent = true)
            where TProviderFilter : class, IProviderTypeFilter
        {
            return builderStep.NewRevision<TypedEntity, TProviderFilter>(name,
                                                                         urlAlias,
                                                                         contentTypeAlias,
                                                                         setRootAsParent);
        }

        /// <summary>
        /// Starts the building of a new entity revision inside the <see cref="IContentStore"/> Hive provider group. Note that changes are not committed to Hive automatically after this step; you must call <see cref="Commit{T, TProviderFilter}"/> or manually add items to a writable Hive unit of work.
        /// </summary>
        /// <param name="hiveManager">The hive manager.</param>
        /// <param name="name">The name.</param>
        /// <param name="urlAlias">The URL alias.</param>
        /// <param name="contentTypeAlias">The content type alias.</param>
        /// <param name="setRootAsParent">if set to <c>true</c> [set root as parent].</param>
        /// <returns></returns>
        public static IContentRevisionBuilderStep<TypedEntity, IContentStore> NewRevision(
            this IRenderViewHiveManagerWrapper hiveManager,
            string name,
            string urlAlias,
            string contentTypeAlias,
            bool setRootAsParent = true)
        {
            return new CmsBuilderStep<IContentStore>(hiveManager).NewRevision<TypedEntity, IContentStore>(name,
                                                                                                          urlAlias,
                                                                                                          contentTypeAlias,
                                                                                                          setRootAsParent);
        }

        /// <summary>
        /// Starts the building of a new entity revision inside the <see cref="IContentStore"/> Hive provider group. Note that changes are not committed to Hive automatically after this step; you must call <see cref="Commit{T, TProviderFilter}"/> or manually add items to a writable Hive unit of work.
        /// </summary>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="builderStep">The builder step.</param>
        /// <param name="name">The name.</param>
        /// <param name="urlAlias">The URL alias.</param>
        /// <param name="contentTypeBuilder">The content type builder.</param>
        /// <param name="setRootAsParent">if set to <c>true</c> [set root as parent].</param>
        /// <returns></returns>
        public static IContentRevisionBuilderStep<TypedEntity, TProviderFilter> NewRevision<TProviderFilter>(
            this ICmsBuilderStep<TProviderFilter> builderStep,
            string name,
            string urlAlias,
            ISchemaBuilderStep<EntitySchema, TProviderFilter> contentTypeBuilder,
            bool setRootAsParent = true)
            where TProviderFilter : class, IProviderTypeFilter
        {
            return builderStep.NewRevision<TypedEntity, TProviderFilter>(name,
                                                                         urlAlias,
                                                                         contentTypeBuilder,
                                                                         setRootAsParent);
        }

        /// <summary>
        /// Starts the building of a new revision of an existing entity inside the <see cref="IContentStore"/> Hive provider group. Note that changes are not committed to Hive automatically after this step; you must call <see cref="Commit{T, TProviderFilter}"/> or manually add items to a writable Hive unit of work.
        /// </summary>
        /// <param name="cmsHiveManager">The CMS hive manager.</param>
        /// <param name="content">The existing content.</param>
        /// <returns></returns>
        public static IContentRevisionBuilderStep<TypedEntity, IContentStore> NewRevisionOf(
            this IRenderViewHiveManagerWrapper cmsHiveManager,
            TypedEntity content)
        {
            return cmsHiveManager.NewRevisionOf<IContentStore>(content);
        }

        /// <summary>
        /// Starts the building of a new revision of an existing entity inside the <typeparamref name="TProviderFilter"/> Hive provider group. Note that changes are not committed to Hive automatically after this step; you must call <see cref="Commit{T, TProviderFilter}"/> or manually add items to a writable Hive unit of work.
        /// </summary>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="cmsHiveManager">The CMS hive manager.</param>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public static IContentRevisionBuilderStep<TypedEntity, TProviderFilter> NewRevisionOf<TProviderFilter>(
            this IRenderViewHiveManagerWrapper cmsHiveManager,
            TypedEntity content)
            where TProviderFilter : class, IProviderTypeFilter
        {
            return cmsHiveManager.NewRevisionOf<TypedEntity, TProviderFilter>(content);
        }

        /// <summary>
        /// Starts the building of a new revision of an existing entity inside the <typeparamref name="TProviderFilter"/> Hive provider group. Note that changes are not committed to Hive automatically after this step; you must call <see cref="Commit{T, TProviderFilter}"/> or manually add items to a writable Hive unit of work.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="cmsHiveManager">The CMS hive manager.</param>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public static IContentRevisionBuilderStep<T, TProviderFilter> NewRevisionOf<T, TProviderFilter>(
            this IRenderViewHiveManagerWrapper cmsHiveManager,
            T content)
            where TProviderFilter : class, IProviderTypeFilter
            where T : TypedEntity, new()
        {
            return new ContentRevisionBuilderStep<T, TProviderFilter>(cmsHiveManager, content);
        }

        /// <summary>
        /// Starts the building of a new entity revision inside the <typeparamref name="TProviderFilter"/> Hive provider group. Note that changes are not committed to Hive automatically after this step; you must call <see cref="Commit{T, TProviderFilter}"/> or manually add items to a writable Hive unit of work.
        /// </summary>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="builderStep">The builder step.</param>
        /// <param name="name">The name.</param>
        /// <param name="urlAlias">The URL alias.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="setRootAsParent">If set to <c>true</c>, automatically set the content root as its parent.</param>
        /// <returns></returns>
        public static IContentRevisionBuilderStep<TypedEntity, TProviderFilter> NewRevision<TProviderFilter>(
            this ICmsBuilderStep<TProviderFilter> builderStep,
            string name,
            string urlAlias,
            EntitySchema contentType,
            bool setRootAsParent = true)
            where TProviderFilter : class, IProviderTypeFilter
        {
            return builderStep.NewRevision<TypedEntity, TProviderFilter>(name, urlAlias, contentType, setRootAsParent);
        }

        /// <summary>
        /// Starts the building of a new entity revision inside the <typeparamref name="TProviderFilter"/> Hive provider group. Note that changes are not committed to Hive automatically after this step; you must call <see cref="Commit{T, TProviderFilter}"/> or manually add items to a writable Hive unit of work.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="builderStep">The builder step.</param>
        /// <param name="name">The name.</param>
        /// <param name="urlAlias">The URL alias.</param>
        /// <param name="contentTypeAlias">The content type alias.</param>
        /// <param name="setRootAsParent">If set to <c>true</c>, automatically set the content root as its parent.</param>
        /// <returns></returns>
        public static IContentRevisionBuilderStep<T, TProviderFilter> NewRevision<T, TProviderFilter>(
            this ICmsBuilderStep<TProviderFilter> builderStep,
            string name,
            string urlAlias,
            string contentTypeAlias,
            bool setRootAsParent = true)
            where TProviderFilter : class, IProviderTypeFilter
            where T : TypedEntity, new()
        {
            var getContentType = builderStep.GetContentTypeByAlias(contentTypeAlias);
            if (getContentType == null)
                throw new ArgumentOutOfRangeException(
                    "Could not find a content type with the alias '{0}'".InvariantFormat(contentTypeAlias));
            return builderStep.NewRevision<T, TProviderFilter>(name, urlAlias, getContentType, setRootAsParent);
        }

        /// <summary>
        /// Sets the revision builder to mark the entity as having a "published" revision status when eventually committed to Hive.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="builderStep">The builder step.</param>
        /// <returns></returns>
        public static IContentRevisionBuilderStep<T, TProviderFilter> Publish<T, TProviderFilter>(
            this IContentRevisionBuilderStep<T, TProviderFilter> builderStep)
            where TProviderFilter : class, IProviderTypeFilter
            where T : TypedEntity, new()
        {
            builderStep.Revision.MetaData.StatusType = FixedStatusTypes.Published;
            return builderStep;
        }

        /// <summary>
        /// Sets the revision builder to mark the entity as having a "unpublished" revision status when eventually committed to Hive.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="builderStep">The builder step.</param>
        /// <returns></returns>
        public static IContentRevisionBuilderStep<T, TProviderFilter> UnPublish<T, TProviderFilter>(
            this IContentRevisionBuilderStep<T, TProviderFilter> builderStep)
            where TProviderFilter : class, IProviderTypeFilter
            where T : TypedEntity, new()
        {
            builderStep.Revision.MetaData.StatusType = FixedStatusTypes.Unpublished;
            return builderStep;
        }

        /// <summary>
        /// Starts the building of a new entity revision inside the <typeparamref name="TProviderFilter"/> Hive provider group. Note that changes are not committed to Hive automatically after this step; you must call <see cref="Commit{T, TProviderFilter}"/> or manually add items to a writable Hive unit of work.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="builderStep">The builder step.</param>
        /// <param name="name">The name.</param>
        /// <param name="urlAlias">The URL alias.</param>
        /// <param name="contentTypeBuilder">The content type builder.</param>
        /// <param name="setRootAsParent">If set to <c>true</c>, automatically set the content root as its parent.</param>
        /// <returns></returns>
        public static IContentRevisionBuilderStep<T, TProviderFilter> NewRevision<T, TProviderFilter>(
            this ICmsBuilderStep<TProviderFilter> builderStep,
            string name,
            string urlAlias,
            ISchemaBuilderStep<EntitySchema, TProviderFilter> contentTypeBuilder,
            bool setRootAsParent = true)
            where TProviderFilter : class, IProviderTypeFilter
            where T : TypedEntity, new()
        {
            Mandate.ParameterNotNull(contentTypeBuilder, "contentTypeBuilder");
            return builderStep.NewRevision<T, TProviderFilter>(name, urlAlias, contentTypeBuilder.Item, setRootAsParent);
        }

        /// <summary>
        /// Starts the building of a new entity revision inside the <typeparamref name="TProviderFilter"/> Hive provider group. Note that changes are not committed to Hive automatically after this step; you must call <see cref="Commit{T, TProviderFilter}"/> or manually add items to a writable Hive unit of work.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="builderStep">The builder step.</param>
        /// <param name="name">The name.</param>
        /// <param name="urlAlias">The URL alias.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="setRootAsParent">If set to <c>true</c>, automatically set the content root as its parent.</param>
        /// <returns></returns>
        public static IContentRevisionBuilderStep<T, TProviderFilter> NewRevision<T, TProviderFilter>(
            this ICmsBuilderStep<TProviderFilter> builderStep,
            string name,
            string urlAlias,
            EntitySchema contentType,
            bool setRootAsParent = true)
            where TProviderFilter : class, IProviderTypeFilter
            where T : TypedEntity, new()
        {
            Mandate.ParameterNotNull(contentType, "contentType");
            var newContent = new T();
            newContent.SetupFromSchema(contentType);
            newContent.Attributes[NodeNameAttributeDefinition.AliasValue].Values["Name"] = name;
            newContent.Attributes[NodeNameAttributeDefinition.AliasValue].Values["UrlName"] = urlAlias;

            // Get the default template for the content type
            var defaultTemplate = contentType.GetXmlConfigProperty<HiveId>("default-template");
            if (defaultTemplate != HiveId.Empty)
            {
                newContent.Attributes[SelectedTemplateAttributeDefinition.AliasValue].Values.SetDefaultValue(
                    defaultTemplate);
            }

            return new ContentRevisionBuilderStep<T, TProviderFilter>(builderStep.HiveManager, newContent)
                {
                    RelateToRoot = setRootAsParent
                };
        }

        /// <summary>
        /// Sets the name of the content being built.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="builderStep">The builder step.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static IContentRevisionBuilderStep<TModel, TProviderFilter> SetName<TModel, TProviderFilter>(
            this IContentRevisionBuilderStep<TModel, TProviderFilter> builderStep,
            string name)
            where TProviderFilter : class, IProviderTypeFilter
            where TModel : TypedEntity, new()
        {
            builderStep.SetValue(NodeNameAttributeDefinition.AliasValue, name, "Name");
            return builderStep;
        }

        /// <summary>
        /// Sets the URL alias of the content buing built.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="builderStep">The builder step.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static IContentRevisionBuilderStep<TModel, TProviderFilter> SetUrlName<TModel, TProviderFilter>(
            this IContentRevisionBuilderStep<TModel, TProviderFilter> builderStep,
            string name)
            where TProviderFilter : class, IProviderTypeFilter
            where TModel : TypedEntity, new()
        {
            builderStep.SetValue(NodeNameAttributeDefinition.AliasValue, name, "UrlName");
            return builderStep;
        }

        /// <summary>
        /// Sets the parent of the content being built.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="builderStep">The builder step.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="relationType">Type of the relation.</param>
        /// <param name="ordinal">The ordinal of the relation.</param>
        /// <param name="metaData">The relation meta data.</param>
        /// <returns></returns>
        public static IContentRevisionBuilderStep<TModel, TProviderFilter> SetParent<TModel, TProviderFilter>(
            this IContentRevisionBuilderStep<TModel, TProviderFilter> builderStep,
            IRelatableEntity parent,
            AbstractRelationType relationType = null,
            int ordinal = 0,
            params RelationMetaDatum[] metaData)
            where TProviderFilter : class, IProviderTypeFilter
            where TModel : TypedEntity, new()
        {
            Mandate.ParameterNotNull(parent, "parent");
            return builderStep.SetParent(parent.Id, relationType, ordinal, metaData);
        }

        /// <summary>
        /// Sets the parent of the content being built.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="builderStep">The builder step.</param>
        /// <param name="parentIdAsString">The parent id as string.</param>
        /// <param name="relationType">Type of the relation.</param>
        /// <param name="ordinal">The ordinal of the relation.</param>
        /// <param name="metaData">The relation meta data.</param>
        /// <returns></returns>
        public static IContentRevisionBuilderStep<TModel, TProviderFilter> SetParent<TModel, TProviderFilter>(
            this IContentRevisionBuilderStep<TModel, TProviderFilter> builderStep,
            string parentIdAsString,
            AbstractRelationType relationType = null,
            int ordinal = 0,
            params RelationMetaDatum[] metaData)
            where TProviderFilter : class, IProviderTypeFilter
            where TModel : TypedEntity, new()
        {
            var tryParse = HiveId.TryParse(parentIdAsString);
            if (!tryParse.Success)
                throw new ArgumentOutOfRangeException(
                    "Could not determine an id from the provided string '{0}'".InvariantFormat(parentIdAsString),
                    tryParse.Error);
            return builderStep.SetParent(tryParse.Result, relationType, ordinal, metaData);
        }

        /// <summary>
        /// Sets the parent of the content being built.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="builderStep">The builder step.</param>
        /// <param name="parentId">The parent id.</param>
        /// <param name="relationType">Type of the relation.</param>
        /// <param name="ordinal">The ordinal of the relation.</param>
        /// <param name="metaData">The relation meta data.</param>
        /// <returns></returns>
        public static IContentRevisionBuilderStep<TModel, TProviderFilter> SetParent<TModel, TProviderFilter>(
            this IContentRevisionBuilderStep<TModel, TProviderFilter> builderStep,
            HiveId parentId,
            AbstractRelationType relationType = null,
            int ordinal = 0,
            params RelationMetaDatum[] metaData)
            where TProviderFilter : class, IProviderTypeFilter
            where TModel : TypedEntity, new()
        {
            if (relationType == null) relationType = FixedRelationTypes.DefaultRelationType;
            // Turn off the default relation to the root
            builderStep.RelateToRoot = false;
            builderStep.Item.RelationProxies.EnlistParentById(parentId, relationType, ordinal, metaData);
            return builderStep;
        }

        /// <summary>
        /// Sets the selected template of the content being built.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="builderStep">The builder step.</param>
        /// <param name="templateId">The template id.</param>
        /// <returns></returns>
        public static IContentRevisionBuilderStep<TModel, TProviderFilter> SetSelectedTemplate<TModel, TProviderFilter>(
            this IContentRevisionBuilderStep<TModel, TProviderFilter> builderStep,
            HiveId? templateId)
            where TProviderFilter : class, IProviderTypeFilter
            where TModel : TypedEntity, new()
        {
            builderStep.SetValue(SelectedTemplateAttributeDefinition.AliasValue, templateId);
            return builderStep;
        }

        /// <summary>
        /// Sets the selected template of the content being built.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="builderStep">The builder step.</param>
        /// <param name="relativePath">The relative path of the template.</param>
        /// <returns></returns>
        public static IContentRevisionBuilderStep<TModel, TProviderFilter> SetSelectedTemplate<TModel, TProviderFilter>(
            this IContentRevisionBuilderStep<TModel, TProviderFilter> builderStep,
            string relativePath)
            where TProviderFilter : class, IProviderTypeFilter
            where TModel : TypedEntity, new()
        {
            var attemptTuple = HiveId.TryParse(relativePath);
            if (!attemptTuple.Success)
                throw new ArgumentOutOfRangeException(
                    "Could not determine an id from the relative path '{0}'".InvariantFormat(relativePath),
                    attemptTuple.Error);
            return builderStep.SetSelectedTemplate(attemptTuple.Result);
        }

        /// <summary>
        /// Saves an uploaded file to to Hive group specified by <see cref="ICmsUploadFileStore"/>, and adds a reference to it to the specified file-uploader property of the content being built.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="builderStep">The builder step.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="uploadedFile">The uploaded file.</param>
        /// <param name="errorIfFileNull">if set to <c>true</c> [error if file null].</param>
        /// <returns></returns>
        /// <remarks>Since transactions cannot be shared between the filestore provider and the contentstore provider, the file may be saved even if the content transaction is eventually abandoned or rolled back.</remarks>
        public static IContentRevisionBuilderStep<TModel, TProviderFilter> SetUploadedFile<TModel, TProviderFilter>(
            this IContentRevisionBuilderStep<TModel, TProviderFilter> builderStep,
            string alias,
            HttpPostedFileBase uploadedFile,
            bool errorIfFileNull = false)
            where TProviderFilter : class, IProviderTypeFilter
            where TModel : TypedEntity, new()
        {
            Mandate.ParameterNotNull(builderStep, "builderStep");
            if (builderStep.Revision.Item == null)
                throw new ArgumentNullException("builderStep.Revision.Item is null - ensure you call this method after calling NewRevision");

            if (errorIfFileNull)
                Mandate.ParameterNotNull(uploadedFile, "uploadedFile");

            var sizes = "";

            using (var uow = builderStep.HiveManager.OpenReader<IContentStore>())
            {
                var uploadPropertyEditor =
                    uow.Repositories.Schemas.Get<AttributeType>(FixedHiveIds.FileUploadAttributeType);
                if (uploadPropertyEditor != null)
                {
                    var dataType =
                        builderStep.HiveManager.Context.FrameworkContext.TypeMappers.Map<AttributeType, DataType>(
                            uploadPropertyEditor);
                    if (dataType != null)
                    {
                        var preValueModel = dataType.GetPreValueModel();
                        if (preValueModel != null)
                        {
                            // This is a bit ugly, but not sure if the Cms.Web project should have a reference to the
                            // file upload property editor to safely cast the pre value model as an UploadPreValueModel,
                            // so in the mean time, I'm just using reflection to get the Sizes property
                            try
                            {
                                sizes =
                                    preValueModel.GetType().GetProperty("Sizes").GetValue(preValueModel, null).ToString();
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
            }

            var unitFactory = builderStep.HiveManager.GetWriter<ICmsUploadFileStore>();

            var valueDict = ContentExtensions.WriteUploadedFile(Guid.NewGuid(),
                                                                false,
                                                                uploadedFile,
                                                                unitFactory,
                                                                default(HiveId),
                                                                sizes);

            return builderStep.SetValue(alias, valueDict);
        }

        /// <summary>
        /// Sets the value of a property on the content being built.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="builderStep">The builder step.</param>
        /// <param name="alias">The property alias.</param>
        /// <param name="value">The value.</param>
        /// <param name="subAlias">The inner property alias.</param>
        /// <returns></returns>
        public static IContentRevisionBuilderStep<TModel, TProviderFilter> SetValue<TModel, TValue, TProviderFilter>(
            this IContentRevisionBuilderStep<TModel, TProviderFilter> builderStep,
            string alias,
            TValue value,
            string subAlias = null)
            where TProviderFilter : class, IProviderTypeFilter
            where TModel : TypedEntity, new()
        {
            Mandate.ParameterNotNull(builderStep, "builderStep");
            if (builderStep.Revision.Item == null)
                throw new ArgumentNullException(
                    "builderStep.Revision.Item is null - ensure you call this method after calling NewRevision");

            if (!builderStep.Revision.Item.Attributes.ContainsKey(alias))
            {
                var attribDefinition =
                    builderStep.Revision.Item.EntitySchema.AttributeDefinitions.FirstOrDefault(x => x.Alias == alias);
                if (attribDefinition == null)
                {
                    var composite = builderStep.Revision.Item.EntitySchema as CompositeEntitySchema;
                    if (composite != null)
                    {
                        attribDefinition = composite.AllAttributeDefinitions.FirstOrDefault(x => x.Alias == alias);
                    }
                    else
                    {
                        // We might be in the middle of building these schemas, so check the parent proxies
                        var allParentRelations = builderStep.Revision.Item.EntitySchema
                            .RelationProxies
                            .GetManualParentProxies()
                            .Select(x => x.Item.Source)
                            .OfType<EntitySchema>()
                            .WhereNotNull().ToArray();
                        if (allParentRelations.Length > 0)
                        {
                            composite = new CompositeEntitySchema(builderStep.Revision.Item.EntitySchema, allParentRelations, true);
                            builderStep.Revision.Item.EntitySchema = composite;
                            attribDefinition = composite.AllAttributeDefinitions.FirstOrDefault(x => x.Alias == alias);
                        }
                    }
                }
                if (attribDefinition == null)
                    throw new ArgumentNullException("The content type for this revision ({0}) does not have a property with alias '{1}'".
                            InvariantFormat(builderStep.Revision.Item.EntitySchema.Alias, alias));
                builderStep.Revision.Item.Attributes.Add(new TypedAttribute(attribDefinition));
            }

            // If the incoming value is a dictionary, automatically add the subalias values
            if (TypeFinder.IsTypeAssignableFrom<IDictionary<string, object>>(value))
            {
                var asDict = value as IDictionary<string, object>;
                if (asDict != null)
                    foreach (var kv in asDict)
                    {
                        builderStep.Revision.Item.Attributes[alias].Values[kv.Key] = kv.Value;
                    }
            }
            else
            {
                if (subAlias == null)
                {
                    builderStep.Revision.Item.Attributes[alias].Values.SetDefaultValue(value);
                }
                else
                {
                    builderStep.Revision.Item.Attributes[alias].Values.AddOrUpdate(subAlias, value);
                }
            }
            return builderStep;
        }

        /// <summary>
        /// Starts building a new content type in Hive providers matching the <typeparamref name="TProviderFilter"/> group. Note that changes are not committed to Hive automatically after this step; you must call <see cref="Commit{T, TProviderFilter}"/> or manually add items to a writable Hive unit of work.
        /// </summary>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="builderStep">The builder step.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static ISchemaBuilderStep<EntitySchema, TProviderFilter> NewContentType<TProviderFilter>(
            this ICmsBuilderStep<TProviderFilter> builderStep,
            string alias,
            string name = null)
            where TProviderFilter : class, IProviderTypeFilter
        {
            return builderStep.NewContentType<EntitySchema, TProviderFilter>(alias, name);
        }

        /// <summary>
        /// Starts building a new content type in Hive providers matching the <see cref="IContentStore"/> group. Note that changes are not committed to Hive automatically after this step; you must call <see cref="Commit{T, TProviderFilter}"/> or manually add items to a writable Hive unit of work.
        /// </summary>
        /// <param name="hiveManager">The hive manager.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static ISchemaBuilderStep<EntitySchema, IContentStore> NewContentType(
            this IRenderViewHiveManagerWrapper hiveManager,
            string alias,
            string name = null)
        {
            return hiveManager.NewContentType<EntitySchema, IContentStore>(alias, name);
        }

        /// <summary>
        /// Starts building a new content type in Hive providers matching the <typeparamref name="TProviderFilter"/> group. Note that changes are not committed to Hive automatically after this step; you must call <see cref="Commit{T, TProviderFilter}"/> or manually add items to a writable Hive unit of work.
        /// </summary>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="hiveManager">The hive manager.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static ISchemaBuilderStep<EntitySchema, TProviderFilter> NewContentType<TProviderFilter>(
            this IRenderViewHiveManagerWrapper hiveManager,
            string alias,
            string name = null)
            where TProviderFilter : class, IProviderTypeFilter
        {
            return hiveManager.NewContentType<EntitySchema, TProviderFilter>(alias, name);
        }

        /// <summary>
        /// Starts building a new content type in Hive providers matching the <typeparamref name="TProviderFilter"/> group. Note that changes are not committed to Hive automatically after this step; you must call <see cref="Commit{T, TProviderFilter}"/> or manually add items to a writable Hive unit of work.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="hiveManager">The hive manager.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static ISchemaBuilderStep<T, TProviderFilter> NewContentType<T, TProviderFilter>(
            this IRenderViewHiveManagerWrapper hiveManager,
            string alias,
            string name = null)
            where TProviderFilter : class, IProviderTypeFilter
            where T : EntitySchema, new()
        {
            return new CmsBuilderStep<TProviderFilter>(hiveManager).NewContentType<T, TProviderFilter>(alias, name);
        }

        /// <summary>
        /// Starts building a new content type in Hive providers matching the <typeparamref name="TProviderFilter"/> group. Note that changes are not committed to Hive automatically after this step; you must call <see cref="Commit{T, TProviderFilter}"/> or manually add items to a writable Hive unit of work.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="builderStep">The builder step.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static ISchemaBuilderStep<T, TProviderFilter> NewContentType<T, TProviderFilter>(
            this ICmsBuilderStep<TProviderFilter> builderStep,
            string alias,
            string name = null)
            where TProviderFilter : class, IProviderTypeFilter
            where T : EntitySchema, new()
        {
            var schema = builderStep.HiveManager.NewSchema<T, TProviderFilter>(alias, name);
            // Add the required minimum properties for content schemas, including the general group
            var existingOrNewGeneralGroup =
                schema.Item.AttributeGroups.Where(x => x.Alias == FixedGroupDefinitions.GeneralGroupAlias).
                    SingleOrDefault() ?? FixedGroupDefinitions.GeneralGroup;
            schema.Item.AttributeGroups.Add(existingOrNewGeneralGroup);
            schema.Item.TryAddAttributeDefinition(new NodeNameAttributeDefinition(existingOrNewGeneralGroup));
            schema.Item.TryAddAttributeDefinition(new SelectedTemplateAttributeDefinition(existingOrNewGeneralGroup));
            schema.Item.RelationProxies.EnlistParentById(FixedHiveIds.ContentRootSchema,
                                                         FixedRelationTypes.DefaultRelationType);
            return schema;
        }

        /// <summary>
        /// Adds a permitted template to the content type being built.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="builderStep">The builder step.</param>
        /// <param name="templateId">The template id.</param>
        /// <returns></returns>
        public static ISchemaBuilderStep<T, TProviderFilter> AddPermittedTemplate<T, TProviderFilter>(
            this ISchemaBuilderStep<T, TProviderFilter> builderStep,
            string templateId)
            where TProviderFilter : class, IProviderTypeFilter
            where T : EntitySchema, new()
        {
            var templateAsHiveId = HiveId.TryParse(templateId);
            if (!templateAsHiveId.Success)
                throw new ArgumentOutOfRangeException("templateId", templateId, templateAsHiveId.Error.Message);
            return builderStep.AddPermittedTemplate(templateAsHiveId.Result);
        }

        /// <summary>
        /// Clears the permitted templates of the content type being built.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="builderStep">The builder step.</param>
        /// <returns></returns>
        public static ISchemaBuilderStep<T, TProviderFilter> ClearPermittedTemplates<T, TProviderFilter>(
            this ISchemaBuilderStep<T, TProviderFilter> builderStep)
            where TProviderFilter : class, IProviderTypeFilter
            where T : EntitySchema, new()
        {
            Mandate.ParameterNotNull(builderStep, "builderStep");

            builderStep.Item.SetXmlConfigProperty("allowed-templates", new HiveId[] { });

            return builderStep;
        }

        /// <summary>
        /// Sets the default template of the content type being built.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="builderStep">The builder step.</param>
        /// <param name="templateId">The template id.</param>
        /// <returns></returns>
        public static ISchemaBuilderStep<T, TProviderFilter> SetDefaultTemplate<T, TProviderFilter>(
            this ISchemaBuilderStep<T, TProviderFilter> builderStep,
            string templateId)
            where TProviderFilter : class, IProviderTypeFilter
            where T : EntitySchema, new()
        {
            Mandate.ParameterNotNull(builderStep, "builderStep");
            var templateAsHiveId = HiveId.TryParse(templateId);
            if (!templateAsHiveId.Success)
                throw new ArgumentOutOfRangeException("templateId", templateId, templateAsHiveId.Error.Message);

            return builderStep.SetDefaultTemplate(templateAsHiveId.Result);
        }

        /// <summary>
        /// Sets the default template of the content type being built.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="builderStep">The builder step.</param>
        /// <param name="templateId">The template id.</param>
        /// <returns></returns>
        public static ISchemaBuilderStep<T, TProviderFilter> SetDefaultTemplate<T, TProviderFilter>(
            this ISchemaBuilderStep<T, TProviderFilter> builderStep,
            HiveId templateId)
            where TProviderFilter : class, IProviderTypeFilter
            where T : EntitySchema, new()
        {
            Mandate.ParameterNotNull(builderStep, "builderStep");
            Mandate.ParameterNotEmpty(templateId, "templateId");

            var templates = builderStep.Item.GetXmlPropertyAsList<HiveId>("allowed-templates");

            if (!templates.Select(x => x.Value).Contains(templateId.Value))
                throw new ArgumentOutOfRangeException(
                    "Cannot set '{0}' as a default template as it is not added as an allowed template.".InvariantFormat(
                        templateId.Value));

            builderStep.Item.SetXmlConfigProperty("default-template", templateId);

            return builderStep;
        }

        /// <summary>
        /// Adds a permitted template to the content type being built.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="builderStep">The builder step.</param>
        /// <param name="templateId">The template id.</param>
        /// <returns></returns>
        public static ISchemaBuilderStep<T, TProviderFilter> AddPermittedTemplate<T, TProviderFilter>(
            this ISchemaBuilderStep<T, TProviderFilter> builderStep,
            HiveId templateId)
            where TProviderFilter : class, IProviderTypeFilter
            where T : EntitySchema, new()
        {
            Mandate.ParameterNotNull(builderStep, "builderStep");
            Mandate.ParameterNotEmpty(templateId, "templateId");
            // Get the existing template ids, if any
            var existingAllowedTemplates = builderStep.Item.GetXmlPropertyAsList<HiveId>("allowed-templates");
            if (existingAllowedTemplates.Contains(templateId)) return builderStep;
            var addTemplate = new List<HiveId>(existingAllowedTemplates)
                {
                    templateId
                };
            builderStep.Item.SetXmlConfigProperty("allowed-templates", addTemplate.ToArray());
            return builderStep;
        }

        /// <summary>
        /// Starts building a new media type in Hive providers matching the <typeparamref name="TProviderFilter"/> group. Note that changes are not committed to Hive automatically after this step; you must call <see cref="Commit{T, TProviderFilter}"/> or manually add items to a writable Hive unit of work.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="cmsBuilderStep">The CMS builder step.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static ISchemaBuilderStep<T, TProviderFilter> NewMediaType<T, TProviderFilter>(
            this ICmsBuilderStep<TProviderFilter> cmsBuilderStep,
            string alias,
            string name = null)
            where TProviderFilter : class, IProviderTypeFilter
            where T : EntitySchema, new()
        {
            var schema = cmsBuilderStep.HiveManager.NewSchema<T, TProviderFilter>(alias);
            schema.Item.RelationProxies.EnlistParentById(FixedHiveIds.MediaRootSchema,
                                                         FixedRelationTypes.DefaultRelationType);
            return schema;
        }

        /// <summary>
        /// Gets a content type by its alias.
        /// </summary>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="builderStep">The builder step.</param>
        /// <param name="alias">The alias.</param>
        /// <returns></returns>
        public static EntitySchema GetContentTypeByAlias<TProviderFilter>(
            this ICmsBuilderStep<TProviderFilter> builderStep,
            string alias)
            where TProviderFilter : class, IProviderTypeFilter
        {
            return builderStep.GetContentTypeByAlias<EntitySchema, TProviderFilter>(alias);
        }

        /// <summary>
        /// Gets a content type by its alias.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="builderStep">The builder step.</param>
        /// <param name="alias">The alias.</param>
        /// <returns></returns>
        public static CompositeEntitySchema GetContentTypeByAlias<T, TProviderFilter>(
            this ICmsBuilderStep<TProviderFilter> builderStep,
            string alias)
            where TProviderFilter : class, IProviderTypeFilter
            where T : EntitySchema, new()
        {
            return builderStep.HiveManager.GetSchemaByAlias<TProviderFilter>(alias);
        }

        /// <summary>
        /// Gets a content type by its id.
        /// </summary>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="builderStep">The builder step.</param>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static CompositeEntitySchema GetContentTypeById<TProviderFilter>(
            this ICmsBuilderStep<TProviderFilter> builderStep,
            HiveId id)
            where TProviderFilter : class, IProviderTypeFilter
        {
            return builderStep.GetContentTypeById<EntitySchema, TProviderFilter>(id);
        }

        /// <summary>
        /// Gets a content type by its id.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="builderStep">The builder step.</param>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static CompositeEntitySchema GetContentTypeById<T, TProviderFilter>(
            this ICmsBuilderStep<TProviderFilter> builderStep,
            HiveId id)
            where TProviderFilter : class, IProviderTypeFilter
        {
            using (var uow = builderStep.HiveManager.OpenReader<TProviderFilter>())
            {
                return uow.Repositories.Schemas.GetComposite<EntitySchema>(id);
            }
        }

        public static IContentRevisionBuilderStep<T, TProviderFilter> SaveTo<T, TProviderFilter>(
            this IContentRevisionBuilderStep<T, TProviderFilter> modelBuilder,
            IGroupUnit<TProviderFilter> writer)
            where TProviderFilter : class, IProviderTypeFilter
            where T : TypedEntity, new()
        {
            Mandate.ParameterNotNull(modelBuilder, "modelBuilder");
            Mandate.ParameterNotNull(writer, "writer");
            Mandate.ParameterCondition(!writer.WasAbandoned, "writer.WasAbandoned");

            if (modelBuilder.RelateToRoot)
            {
                modelBuilder.Revision.Item.RelationProxies.EnlistParentById(FixedHiveIds.ContentVirtualRoot,
                                                                            FixedRelationTypes.DefaultRelationType);
            }

            var item = modelBuilder.Revision;

            writer.Repositories.Revisions.AddOrUpdate(item);

            // Workaround: After calling Revisions.AddOrUpdate, the Nh provider remaps the object which means
            // EntitySchema is reset from a composite to a normal. Reload the composite schema here before returning
            var composite = writer.Repositories.Schemas.GetComposite<EntitySchema>(item.Item.EntitySchema.Id);
            if (composite != null) item.Item.EntitySchema = composite;

            return modelBuilder;
        }

        /// <summary>
        /// Commits the specified model builder to Hive.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="modelBuilder">The model builder.</param>
        /// <returns>An <see cref="IContentRevisionSaveResult{TypedEntity}"/> with information about whether the commital was successful, and if not, what errors were raised. The result also has a reference to the item that was built.</returns>
        public static IContentRevisionSaveResult<TypedEntity> Commit<T, TProviderFilter>(
            this IContentRevisionBuilderStep<T, TProviderFilter> modelBuilder)
            where TProviderFilter : class, IProviderTypeFilter
            where T : TypedEntity, new()
        {
            var userId = HiveId.Empty;
            var userService = DependencyResolver.Current.GetService<IMembershipService<User>>();
            if(userService != null)
            {
                var user = userService.GetCurrent();
                if (user != null)
                    userId = user.Id;
            }
            return modelBuilder.Commit(userId);
        }

        /// <summary>
        /// Commits the specified model builder to Hive.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProviderFilter">The type of the provider filter.</typeparam>
        /// <param name="modelBuilder">The model builder.</param>
        /// <param name="userId">The user id.</param>
        /// <returns>
        /// An <see cref="IContentRevisionSaveResult{TypedEntity}"/> with information about whether the commital was successful, and if not, what errors were raised. The result also has a reference to the item that was built.
        /// </returns>
        public static IContentRevisionSaveResult<TypedEntity> Commit<T, TProviderFilter>(
            this IContentRevisionBuilderStep<T, TProviderFilter> modelBuilder,
            HiveId userId)
            where TProviderFilter : class, IProviderTypeFilter
            where T : TypedEntity, new()
        {
            if (modelBuilder.RelateToRoot)
            {
                modelBuilder.Revision.Item.RelationProxies.EnlistParentById(FixedHiveIds.ContentVirtualRoot,
                                                                            FixedRelationTypes.DefaultRelationType);
            }

            using (var unit = modelBuilder.HiveManager.OpenWriter<TProviderFilter>())
            {
                var item = modelBuilder.Revision;
                try
                {
                    if (!userId.IsNullValueOrEmpty())
                    {
                        if (item.Item.Id.IsNullValueOrEmpty())
                        {
                            // New
                            item.Item.RelationProxies.EnlistParentById(userId, FixedRelationTypes.CreatedByRelationType);
                            item.Item.RelationProxies.EnlistParentById(userId, FixedRelationTypes.ModifiedByRelationType);
                        }
                        else
                        {
                            // Modified
                            var mRelations = unit.Repositories.GetParentRelations(item.Item.Id,
                                                                                  FixedRelationTypes.
                                                                                      ModifiedByRelationType);
                            foreach (var relationById in mRelations)
                            {
                                unit.Repositories.RemoveRelation(relationById);
                            }

                            item.Item.RelationProxies.EnlistParentById(userId, FixedRelationTypes.ModifiedByRelationType);
                        }
                    }

                    unit.Repositories.Revisions.AddOrUpdate(item);
                    unit.Complete();
                    // Workaround: After calling Revisions.AddOrUpdate, the Nh provider remaps the object which means
                    // EntitySchema is reset from a composite to a normal. Reload the composite schema here before returning
                    var composite = unit.Repositories.Schemas.GetComposite<EntitySchema>(item.Item.EntitySchema.Id);
                    if (composite != null) item.Item.EntitySchema = composite;
                    return new ContentRevisionSaveResult<TypedEntity>(true, item);
                }
                catch (Exception ex)
                {
                    unit.Abandon();
                    return new ContentRevisionSaveResult<TypedEntity>(false, item, ex);
                }
            }
        }

        #region Nested type: ICmsUploadFileStore

        #endregion
    }
}