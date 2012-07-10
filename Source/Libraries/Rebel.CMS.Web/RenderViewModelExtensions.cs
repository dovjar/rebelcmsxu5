using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model;
using Rebel.Cms.Web.Routing;
using Rebel.Cms.Web.System;
using Rebel.Framework;
using Rebel.Framework.Context;
using Rebel.Framework.Data;
using Rebel.Framework.Dynamics;
using Rebel.Framework.Dynamics.Expressions;
using Rebel.Framework.Linq.CriteriaGeneration.StructureMetadata;
using Rebel.Framework.Persistence;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Attribution;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Rebel.Framework.Persistence.Model.Versioning;
using Rebel.Framework.Persistence.ProviderSupport;
using Rebel.Framework.Security.Model.Entities;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.RepositoryTypes;
using Rebel.Hive;
namespace Rebel.Cms.Web
{
    using Rebel.Cms.Web.Security.Permissions;
    using Rebel.Framework.Persistence.Model.Associations;
    using Rebel.Framework.Security;
    using global::System.Reflection;

    public static class RenderViewModelExtensions
    {

        /// <summary>
        /// Gets the path of the content 
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static EntityPath GetPath(this Content content)
        {
            var appContext = DependencyResolver.Current.GetService<IRebelApplicationContext>();
            if (appContext != null && appContext.Hive != null)
            {
                return GetPath(content, appContext.Hive);
            }
            return new EntityPath(content.Id.AsEnumerableOfOne());
        }

        public static EntityPath GetPath(this Content content, IHiveManager hiveManager)
        {
            // Open a reader and pass in the scoped cache which should be per http-request
            using (var uow = hiveManager.OpenReader<IContentStore>(hiveManager.FrameworkContext.ScopedCache))
            {
                return uow.Repositories.GetEntityPath(content.Id, FixedRelationTypes.DefaultRelationType);
            }
        }

        /// <summary>
        /// Returns the url 
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string NiceUrl(this Content content)
        {
            var urlUtility = DependencyResolver.Current.GetService<IRoutingEngine>();
            return urlUtility.GetUrl(content.Id);
        }

        public static IEnumerable<T> GetAncestors<T>(this IHiveManager hiveManager, HiveId descendentId, RelationType relationType = null, string relationAlias = null)
            where T : class, IRelatableEntity
        {
            // Open a reader and pass in the scoped cache which should be per http-request
            using (var uow = hiveManager.OpenReader<IContentStore>(hiveManager.FrameworkContext.ScopedCache))
            {
                if (relationType != null && relationAlias.IsNullOrWhiteSpace())
                    relationType = new RelationType(relationAlias);

                return uow.Repositories.GetAncestors(descendentId, relationType)
                    .ForEach(x => hiveManager.FrameworkContext.TypeMappers.Map<T>(x));
            }
        }

        public static T Parent<T>(IHiveManager hiveManager, ISecurityService security, HiveId childId)
            where T : TypedEntity
        {
            // Open a reader and pass in the scoped cache which should be per http-request
            using (var uow = hiveManager.OpenReader<IContentStore>(hiveManager.FrameworkContext.ScopedCache))
            {
                // TODO: Add path of current to this

                // We get the relations by id here because we only want to load the parent entity, not both halves of the relation
                // (as of 15h Oct 2011 but this should change once UoW gets its own ScopedCache added to cater for this - APN)
                var firstParentFound = uow.Repositories.GetParentRelations(childId, FixedRelationTypes.DefaultRelationType)
                        .FirstOrDefault();

                if (firstParentFound != null)
                {
                    var parentId = firstParentFound.SourceId.AsEnumerableOfOne();

                    // Filter the ancestor ids based on anonymous view permissions
                    //using (var secUow = hiveManager.OpenReader<ISecurityStore>(hiveManager.FrameworkContext.ScopedCache))
                    //    parentId = parentId.FilterAnonymousWithPermissions(security, uow, secUow, new ViewPermission().Id).ToArray();

                    if (!parentId.Any()) return null;

                    //var parent = uow.Repositories.Get<T>(parentId);
                    //if (parent != null)
                    //    return hiveManager.FrameworkContext.TypeMappers.Map<T>(parent);
                    //var revision = uow.Repositories.Revisions.GetLatestRevision<TypedEntity>(parentId.FirstOrDefault(), FixedStatusTypes.Published);
                    var item = uow.Repositories.Query<T>().OfRevisionType(FixedStatusTypes.Published).InIds(parentId.FirstOrDefault()).FirstOrDefault();
                    return item != null ? hiveManager.FrameworkContext.TypeMappers.Map<T>(item) : null;
                }
            }
            return null;
        }

        public static IEnumerable<Content> AncestorContent(this Content content)
        {
            if (content == null) return Enumerable.Empty<Content>();
            var appContext = DependencyResolver.Current.GetService<IRebelApplicationContext>();
            if (appContext != null && appContext.Hive != null)
            {
                return appContext.FrameworkContext.ScopedCache.GetOrCreateTyped<IEnumerable<Content>>(
                    "rvme_AncestorContent_" + content.Id,
                    () =>
                    {
                        // Open a reader and pass in the scoped cache which should be per http-request
                        using (var uow = appContext.Hive.OpenReader<IContentStore>(appContext.FrameworkContext.ScopedCache))
                        {
                            var hiveIds = uow.Repositories.GetAncestorIds(content.Id, FixedRelationTypes.DefaultRelationType).ToArray();

                            // Filter the ancestor ids based on anonymous view permissions
                            //using (var secUow = appContext.Hive.OpenReader<ISecurityStore>(appContext.FrameworkContext.ScopedCache))
                            //    hiveIds = hiveIds.FilterAnonymousWithPermissions(appContext.Security, uow, secUow, new ViewPermission().Id).ToArray();

                            var ancestors = uow.Repositories.OfRevisionType(FixedStatusTypes.Published).InIds(hiveIds).ToList();

                            //var ancestors = uow.Repositories.Revisions.GetLatestRevisions<TypedEntity>(false, FixedStatusTypes.Published, hiveIds);
                            return ancestors
                                .WhereNotNull()
                                .Where(x => x.EntitySchema != null && !x.EntitySchema.Id.IsSystem())
                                .Select(x => appContext.FrameworkContext.TypeMappers.Map<Content>(x))
                                .ToArray();
                        }
                    });
            }
            return Enumerable.Empty<Content>();
        }

        /// <summary>
        /// Gets all ancestor ids of the <paramref name="entity"/> regardless of publish status.
        /// </summary>
        /// <param name="entity">The content.</param>
        /// <returns></returns>
        public static IEnumerable<HiveId> AllAncestorIds(this TypedEntity entity)
        {
            if (entity == null) return Enumerable.Empty<HiveId>();
            var appContext = DependencyResolver.Current.GetService<IRebelApplicationContext>();
            if (appContext != null && appContext.Hive != null)
            {
                return appContext.FrameworkContext.ScopedCache.GetOrCreateTyped<IEnumerable<HiveId>>(
                    "rvme_AllAncestorIds_" + entity.Id,
                    () =>
                    {
                        // Open a reader and pass in the scoped cache which should be per http-request
                        using (var uow = appContext.Hive.OpenReader<IContentStore>(appContext.FrameworkContext.ScopedCache))
                        {
                            var hiveIds = uow.Repositories.GetAncestorIds(entity.Id, FixedRelationTypes.DefaultRelationType).ToArray();

                            // Filter the ancestor ids based on anonymous view permissions
                            //using (var secUow = appContext.Hive.OpenReader<ISecurityStore>(appContext.FrameworkContext.ScopedCache))
                            //    hiveIds = hiveIds.FilterAnonymousWithPermissions(appContext.Security, uow, secUow, new ViewPermission().Id).ToArray();

                            return hiveIds;
                        }
                    });
            }
            return Enumerable.Empty<HiveId>();
        }

        public static IEnumerable<HiveId> AllAncestorIdsOrSelf(this TypedEntity entity)
        {
            if (entity == null) return Enumerable.Empty<HiveId>();
            return entity.Id.AsEnumerableOfOne().Union(entity.AllAncestorIds());
        }


        /// <summary>
        /// Gets all descendant ids of the <paramref name="entity"/> regardless of publish status.
        /// </summary>
        /// <param name="entity">The content.</param>
        /// <returns></returns>
        public static IEnumerable<HiveId> AllDescendantIds(this TypedEntity entity)
        {
            if (entity == null) return Enumerable.Empty<HiveId>();
            var appContext = DependencyResolver.Current.GetService<IRebelApplicationContext>();
            if (appContext != null && appContext.Hive != null)
            {
                return appContext.FrameworkContext.ScopedCache.GetOrCreateTyped<IEnumerable<HiveId>>(
                    "rvme_AllDescendantIds_" + entity.Id,
                    () =>
                    {
                        // Open a reader and pass in the scoped cache which should be per http-request
                        using (var uow = appContext.Hive.OpenReader<IContentStore>(appContext.FrameworkContext.ScopedCache))
                        {
                            var hiveIds = uow.Repositories.GetDescendantIds(entity.Id, FixedRelationTypes.DefaultRelationType).ToArray();

                            // Filter the ancestor ids based on anonymous view permissions
                            //using (var secUow = appContext.Hive.OpenReader<ISecurityStore>(appContext.FrameworkContext.ScopedCache))
                            //    hiveIds = hiveIds.FilterAnonymousWithPermissions(appContext.Security, uow, secUow, new ViewPermission().Id).ToArray();

                            return hiveIds;
                        }
                    });
            }
            return Enumerable.Empty<HiveId>();
        }

        /// <summary>
        /// Gets all descendant ids of the <paramref name="entity"/> regardless of publish status.
        /// </summary>
        /// <param name="entity">The content.</param>
        /// <returns></returns>
        public static IEnumerable<HiveId> AllChildIds(this TypedEntity entity)
        {
            var appContext = DependencyResolver.Current.GetService<IRebelApplicationContext>();
            if (appContext != null && appContext.Hive != null)
            {
                return entity.AllChildIds(appContext.Hive);
            }
            return Enumerable.Empty<HiveId>();
        }

        /// <summary>
        /// Gets all descendant ids of the <paramref name="entity"/> regardless of publish status.
        /// </summary>
        /// <param name="entity">The content.</param>
        /// <returns></returns>
        public static IEnumerable<HiveId> AllChildIds(this TypedEntity entity, IHiveManager hiveManager)
        {
            if (entity == null) return Enumerable.Empty<HiveId>();
            return hiveManager.FrameworkContext.ScopedCache.GetOrCreateTyped<IEnumerable<HiveId>>(
                "rvme_AllChildIds_" + entity.Id,
                () =>
                {
                    // Open a reader and pass in the scoped cache which should be per http-request
                    using (var uow = hiveManager.OpenReader<IContentStore>(hiveManager.FrameworkContext.ScopedCache))
                    {
                        var hiveIds = uow.Repositories.GetChildRelations(entity.Id, FixedRelationTypes.DefaultRelationType).Select(x => x.DestinationId).ToArray();

                        // Filter the ancestor ids based on anonymous view permissions
                        //using (var secUow = appContext.Hive.OpenReader<ISecurityStore>(appContext.FrameworkContext.ScopedCache))
                        //    hiveIds = hiveIds.FilterAnonymousWithPermissions(appContext.Security, uow, secUow, new ViewPermission().Id).ToArray();

                        return hiveIds;
                    }
                });
            return Enumerable.Empty<HiveId>();
        }

        public static IEnumerable<HiveId> AllDescendantIdsOrSelf(this TypedEntity entity)
        {
            if (entity == null) return Enumerable.Empty<HiveId>();
            return entity.Id.AsEnumerableOfOne().Union(entity.AllDescendantIds());
        }

        public static IEnumerable<Content> DescendantContent(this Content content)
        {
            if (content == null) return Enumerable.Empty<Content>();
            var appContext = DependencyResolver.Current.GetService<IRebelApplicationContext>();
            if (appContext != null && appContext.Hive != null)
            {
                return appContext.FrameworkContext.ScopedCache.GetOrCreateTyped<IEnumerable<Content>>(
                    "rvme_DescendantContent_" + content.Id,
                    () =>
                    {
                        // Open a reader and pass in the scoped cache which should be per http-request
                        using (var uow = appContext.Hive.OpenReader<IContentStore>(appContext.FrameworkContext.ScopedCache))
                        {
                            var hiveIds = uow.Repositories.GetDescendantIds(content.Id, FixedRelationTypes.DefaultRelationType).ToArray();

                            // Filter the ancestor ids based on anonymous view permissions
                            //using (var secUow = appContext.Hive.OpenReader<ISecurityStore>(appContext.FrameworkContext.ScopedCache))
                            //    hiveIds = hiveIds.FilterAnonymousWithPermissions(appContext.Security, uow, secUow, new ViewPermission().Id).ToArray();

                            var descendants = uow.Repositories.Revisions.GetLatestRevisions<TypedEntity>(false, FixedStatusTypes.Published, hiveIds);
                            return descendants
                                .WhereNotNull()
                                .Where(x => x.Item.EntitySchema != null && !x.Item.EntitySchema.Id.IsSystem())
                                .Select(x => appContext.FrameworkContext.TypeMappers.Map<Content>(x.Item))
                                .ToArray();
                        }
                    });
            }
            return Enumerable.Empty<Content>();
        }

        public static IEnumerable<Content> AncestorContentOrSelf(this Content content)
        {
            if (content == null) return Enumerable.Empty<Content>();
            return content.AsEnumerableOfOne().Union(content.AncestorContent());
        }

        public static IEnumerable<Content> DescendantContentOrSelf(this Content content)
        {
            if (content == null) return Enumerable.Empty<Content>();
            return content.AsEnumerableOfOne().Union(content.DescendantContent());
        }

        public static Content ParentContent(this Content content)
        {
            var appContext = DependencyResolver.Current.GetService<IRebelApplicationContext>();
            if (appContext != null && appContext.Hive != null)
            {
                return content.ParentContent(appContext.Hive);
            }
            return null;
        }

        public static Content ParentContent(this Content content, IHiveManager hiveManager)
        {
            if (content == null) return null;

            return hiveManager.FrameworkContext
                .ScopedCache
                .GetOrCreateTyped<Content>("rvme_ParentContent_" + content.Id,
                    () =>
                    {
                        var parent = Parent<Content>(hiveManager, null, content.Id);
                        return parent;
                    });
        }


        public static IQueryable<Content> Children(this Content content)
        {
            var appContext = DependencyResolver.Current.GetService<IRebelApplicationContext>();
            if (appContext != null && appContext.Hive != null)
            {
                return content.Children(appContext.Hive);
            }
            return Enumerable.Empty<Content>().AsQueryable();
        }

        public static IQueryable<Content> Children(this Content content, IHiveManager hiveManager)
        {
            var childIds = content.AllChildIds();
            // Not exactly sure why we need to do it this way, but when done the other way, it doesn't trigger the content type mapper
            // and therfore looses it's sort order
            return hiveManager.Query<TypedEntity, IContentStore>().OfRevisionType(FixedStatusTypes.Published).InIds(childIds)
                .ToList().Select(x => hiveManager.FrameworkContext.TypeMappers.Map<Content>(x)).AsQueryable();
            //return hiveManager.QueryContent().OfRevisionType(FixedStatusTypes.Published).InIds(childIds);
        }

        public static IEnumerable<Content> ChildContent(this Content content)
        {
            if (content == null) return Enumerable.Empty<Content>();

            return content.Children();

            //var appContext = DependencyResolver.Current.GetService<IRebelApplicationContext>();
            //if (appContext != null && appContext.Hive != null)
            //{
            //    return appContext.FrameworkContext.ScopedCache.GetOrCreateTyped<IEnumerable<Content>>(
            //        "rvme_ChildContent_" + content.Id,
            //        () =>
            //        {
            //            // Open a reader and pass in the scoped cache which should be per http-request
            //            using (var uow = appContext.Hive.OpenReader<IContentStore>(appContext.FrameworkContext.ScopedCache))
            //            {
            //                // TODO: Add path of current to this

            //                // Here we just get the child relations by id, and then bulk ask the repo for items with those ids,
            //                // because we're caching the resultset anyway which means we can't yield the result

            //                // Using this method rather than GetLazyChildRelations avoids the Source (our parent) being loaded too
            //                // (as of 15h Oct 2011 but this should change once UoW gets its own ScopedCache added to cater for this - APN)
            //                var childIds = uow.Repositories.GetChildRelations(
            //                    content.Id, FixedRelationTypes.DefaultRelationType)
            //                    .Select(x => x.DestinationId)
            //                    .ToArray();

            //                var children = uow.Repositories.Revisions.GetLatestRevisions<TypedEntity>(false, FixedStatusTypes.Published, childIds);
            //                return children
            //                    .WhereNotNull()
            //                    .Where(x => x.Item.EntitySchema != null && !x.Item.EntitySchema.Id.IsSystem())
            //                    .Select(x => appContext.FrameworkContext.TypeMappers.Map<Content>(x.Item))
            //                    .ToArray();
            //            }
            //        });
            //}
            //return Enumerable.Empty<Content>();
        }

        /// <summary>
        /// Returns a dynamic representation of the provided content object.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public static dynamic AsDynamic(this Content content)
        {
            Mandate.ParameterNotNull(content, "content");

            return content.Bend();
        }

        /// <summary>
        /// Turns a list of content items into a list of dynamic content items
        /// </summary>
        /// <param name="contentItems"></param>
        /// <returns></returns>
        public static IEnumerable<dynamic> AsDynamic(this IEnumerable<Content> contentItems)
        {
            return contentItems.Select(item => item.AsDynamic());
        }

        #region Dynamic

        public static dynamic DynamicField(this TypedEntity coll, string fieldKey)
        {
            return coll.Field(fieldKey, false);
        }

        public static dynamic DynamicField(this TypedEntity coll, string fieldKey, bool recursive)
        {
            return coll.Field(fieldKey, recursive);
        }

        public static dynamic DynamicField(this TypedEntity coll, string fieldKey, string valueField)
        {
            return coll.Field(fieldKey, valueField, false);
        }

        public static dynamic DynamicField(this TypedEntity coll, string fieldKey, string valueField, bool recursive)
        {
            return coll.Field(fieldKey, valueField, recursive);
        }

        #endregion

        #region Generic

        public static T Field<T>(this TypedEntity coll, [MapsToAliasForQuerying]string fieldKey)
        {
            return (T)Convert.ChangeType(coll.Field(fieldKey, false), typeof(T));
        }

        public static T Field<T>(this TypedEntity coll, [MapsToAliasForQuerying]string fieldKey, bool recursive)
        {
            return (T)Convert.ChangeType(coll.Field(fieldKey, recursive), typeof(T));
        }

        public static T Field<T>(this TypedEntity coll, [MapsToAliasForQuerying]string fieldKey, [MapsToInnerAliasForQuerying]string valueField)
        {
            return (T)Convert.ChangeType(coll.Field(fieldKey, valueField, false), typeof(T));
        }

        public static T Field<T>(this TypedEntity coll, [MapsToAliasForQuerying]string fieldKey, [MapsToInnerAliasForQuerying]string valueField, bool recursive)
        {
            return (T)Convert.ChangeType(coll.Field(fieldKey, valueField, recursive), typeof(T));
        }

        #endregion

        #region String

        public static string StringField(this TypedEntity coll, [MapsToAliasForQuerying]string fieldKey)
        {
            return coll.Field<string>(fieldKey, false);
        }

        public static string StringField(this TypedEntity coll, [MapsToAliasForQuerying]string fieldKey, bool recursive)
        {
            return coll.Field<string>(fieldKey, recursive);
        }

        public static string StringField(this TypedEntity coll, [MapsToAliasForQuerying]string fieldKey, [MapsToInnerAliasForQuerying]string valueField)
        {
            return coll.Field<string>(fieldKey, valueField, false);
        }

        public static string StringField(this TypedEntity coll, [MapsToAliasForQuerying]string fieldKey, [MapsToInnerAliasForQuerying]string valueField, bool recursive)
        {
            return coll.Field<string>(fieldKey, valueField, recursive);
        }

        #endregion

        #region Number

        public static Int32 NumberField(this TypedEntity coll, [MapsToAliasForQuerying]string fieldKey)
        {
            return coll.Field<Int32>(fieldKey, false);
        }

        public static Int32 NumberField(this TypedEntity coll, [MapsToAliasForQuerying]string fieldKey, bool recursive)
        {
            return coll.Field<Int32>(fieldKey, recursive);
        }

        public static Int32 NumberField(this TypedEntity coll, [MapsToAliasForQuerying]string fieldKey, [MapsToInnerAliasForQuerying]string valueField)
        {
            return coll.Field<Int32>(fieldKey, valueField, false);
        }

        public static Int32 NumberField(this TypedEntity coll, [MapsToAliasForQuerying]string fieldKey, [MapsToInnerAliasForQuerying]string valueField, bool recursive)
        {
            return coll.Field<Int32>(fieldKey, valueField, recursive);
        }

        #endregion

        #region Boolean

        public static bool BooleanField(this TypedEntity coll, [MapsToAliasForQuerying]string fieldKey)
        {
            return coll.Field<bool>(fieldKey, false);
        }

        public static bool BooleanField(this TypedEntity coll, [MapsToAliasForQuerying]string fieldKey, bool recursive)
        {
            return coll.Field<bool>(fieldKey, recursive);
        }

        public static bool BooleanField(this TypedEntity coll, [MapsToAliasForQuerying]string fieldKey, [MapsToInnerAliasForQuerying]string valueField)
        {
            return coll.Field<bool>(fieldKey, valueField, false);
        }

        public static bool BooleanField(this TypedEntity coll, [MapsToAliasForQuerying]string fieldKey, [MapsToInnerAliasForQuerying]string valueField, bool recursive)
        {
            return coll.Field<bool>(fieldKey, valueField, recursive);
        }

        #endregion

        #region Object

        public static object Field(this TypedEntity coll, [MapsToAliasForQuerying]string fieldKey)
        {
            return coll.Field(fieldKey, false);
        }

        public static object Field(this TypedEntity coll, [MapsToAliasForQuerying]string fieldKey, bool recursive)
        {
            return coll.Field(fieldKey, "", recursive);
        }

        public static object Field(this TypedEntity coll, [MapsToAliasForQuerying]string fieldKey, [MapsToInnerAliasForQuerying]string valueKey)
        {
            return coll.Field(fieldKey, valueKey, false);
        }

        public static object Field(this TypedEntity coll, [MapsToAliasForQuerying]string fieldKey, [MapsToInnerAliasForQuerying]string valueKey, bool recursive)
        {
            var contentToCheck = new List<TypedEntity>(new[] { coll });

            if (recursive)
            {
                var newContent = new Content(coll.Id, coll.Attributes);
                contentToCheck.AddRange(newContent.AncestorContent());
            }

            foreach (var content in contentToCheck)
            {
                var field = content.Attributes.Where(x => x.AttributeDefinition.Alias == fieldKey).FirstOrDefault();
                if (field != null)
                {
                    var defaultValue = (!valueKey.IsNullOrWhiteSpace()) ? field.Values[valueKey] : field.Values.GetDefaultValue();
                    if (defaultValue != null && !defaultValue.ToString().IsNullOrWhiteSpace()) return defaultValue;
                    //TODO: Update to be able to pass in Action<bool> to perform a null check based on specific types rather than just ToString
                }
            }

            return null;
        }

        #endregion

        public static KeyedFieldValue DefaultValue(this IEnumerable<KeyedFieldValue> coll)
        {
            Mandate.ParameterNotNull(coll, "coll");

            KeyedFieldValue tryDefault =
                coll.Where(x => x.Key == TypedAttributeValueCollection.DefaultAttributeValueKey).FirstOrDefault();
            return tryDefault ?? coll.FirstOrDefault();
        }

        public static dynamic Bend(this Content content)
        {
            if (content == null) return null;

            var appContext = DependencyResolver.Current.GetService<IRebelApplicationContext>();
            if (appContext != null && appContext.Hive != null)
            {
                return content.Bend(appContext.Hive, appContext.Security.Members, appContext.Security.PublicAccess);
            }

            return null;
        }

        public static dynamic Bend(this Content content, IHiveManager hiveManager, IMembershipService<Member> membershipService,
            IPublicAccessService publicAccessService, IEnumerable<Assembly> dynamicExtensionAssemblies = null)
        {
            if (content == null) return null;

            var bendy = new BendyObject(content);
            dynamic dynamicBendy = bendy;

            //get all fields that have a value 
            var attributesWithValue = content.Attributes.Where(typedAttribute => typedAttribute.Values.Any()).ToArray();
            foreach (var attrib in attributesWithValue)
            {

                //if the default value doesn't exist, then add to sub bendy... normally this occurs if the 
                // property editor is saving custom values
                var defaultval = attrib.Values.GetDefaultValue();
                if (attrib.Values.Count > 1 || defaultval == null)
                {
                    //03/05/12 (SD). we only need to create one sub bendy, then assign it to each property alias
                    // this used to create 2 of them not sure why as this will just have more overhead.
                    // This also used to create a new bendy object, then iterate over each of the attrib.Values and 
                    //manually assign each value using AddToBendy. This has changed so that we can better support
                    //multi-value properties and because the BendyObject ctor now has support to check for IDictionary
                    //which automatically adds each dictionary key/value as a property.
                    var subBendy = new BendyObject(attrib.Values);
                    AddToBendy(bendy, attrib.AttributeDefinition.Alias, subBendy);

                    subBendy["__OriginalItem"] = attrib;
                    subBendy.AddLazy("__Parent", () => bendy);
                    subBendy["__ParentKey"] = attrib.AttributeDefinition.Alias;

                    BendyObjectExtensionsHelper.ApplyDynamicFieldExtensions(content,
                        attrib.AttributeDefinition.AttributeType.RenderTypeProvider,
                        subBendy,
                        dynamicExtensionAssemblies);

                    BendyObjectExtensionsHelper.ApplyDynamicExtensions<TypedAttribute>(subBendy, dynamicExtensionAssemblies);

                    AddToBendy(bendy, attrib.AttributeDefinition.Alias, subBendy);
                }
                else
                {
                    AddToBendy(bendy, attrib.AttributeDefinition.Alias, defaultval);
                }
            }

            // Now put in rudimentary default values for any that don't have a value
            // TODO: Get the default values from the backoffice/AttributeDefinition/AttributeType involved in this
            foreach (var attrib in content.Attributes.Except(attributesWithValue))
            {
                object defaultValue = null;
                switch (attrib.AttributeDefinition.AttributeType.SerializationType.DataSerializationType)
                {
                    case DataSerializationTypes.Date:
                        defaultValue = DateTime.MinValue;
                        break;
                    case DataSerializationTypes.Guid:
                        defaultValue = Guid.Empty;
                        break;
                    case DataSerializationTypes.SmallInt:
                    case DataSerializationTypes.LargeInt:
                        defaultValue = 0;
                        break;
                    case DataSerializationTypes.Decimal:
                        defaultValue = 0d;
                        break;
                    case DataSerializationTypes.ByteArray:
                        defaultValue = new byte[] { };
                        break;
                    case DataSerializationTypes.Boolean:
                        defaultValue = false;
                        break;
                    case DataSerializationTypes.String:
                    case DataSerializationTypes.LongString:
                        defaultValue = string.Empty;
                        break;
                }
                AddToBendy(bendy, attrib.AttributeDefinition.Alias, defaultValue);
            }

            bendy["__OriginalItem"] = content;

            bendy.WhenItemNotFound = (bent, membername) =>
                {
                    if (membername.IsNullOrWhiteSpace()) return new BendyObject();
                    // Support recursively looking up a property similarly to v4
                    if (membername.StartsWith("_") && membername.ElementAt(1).IsLowerCase())
                    {
                        return content.Field(membername.TrimStart('_'), true);
                    }
                    // Support pluralised document-type aliases inferring a query
                    if (membername.EndsWith("s", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var alias = membername.TrimEnd("s");
                        var up = alias.StartsWith("Ancestor", StringComparison.InvariantCultureIgnoreCase);
                        if (up) alias = alias.TrimStart("Ancestor");
                        dynamic theBendy = bent;

                        return up ? theBendy.AncestorsOrSelf.Where("NodeTypeAlias == @0", alias) : theBendy.DescendantsOrSelf.Where("NodeTypeAlias == @0", alias);
                    }
                    return new BendyObject();
                };

            bendy.AddLazy("AllAncestorIds", () => content.AllAncestorIds());
            bendy.AddLazy("Ancestors", () =>
                {
                    var ancestorIds = content.AllAncestorIds();
                    return new DynamicContentList(content.Id, hiveManager, membershipService, publicAccessService, new BendyObject(), ancestorIds);
                });
            bendy.AddLazy("AllAncestorIdsOrSelf", () => content.AllAncestorIdsOrSelf());
            bendy.AddLazy("AncestorsOrSelf", () =>
                {
                    var ancestorIds = content.AllAncestorIdsOrSelf();
                    return new DynamicContentList(content.Id, hiveManager, membershipService, publicAccessService, new BendyObject(), ancestorIds);
                });
            bendy.AddLazy("AllDescendantIds", () => content.AllDescendantIds());
            bendy.AddLazy("Descendants", () =>
                {
                    var descendantContentIds = content.AllDescendantIds();
                    return new DynamicContentList(content.Id, hiveManager, membershipService, publicAccessService, new BendyObject(), descendantContentIds);
                });
            bendy.AddLazy("AllDescendantIdsOrSelf", () => content.AllDescendantIdsOrSelf());
            bendy.AddLazy("DescendantsOrSelf", () =>
                {
                    var descendantContentIds = content.AllDescendantIdsOrSelf();
                    return new DynamicContentList(content.Id, hiveManager, membershipService, publicAccessService, new BendyObject(), descendantContentIds);
                });
            bendy.AddLazy("AllChildIds", () => content.AllChildIds(hiveManager));
            bendy.AddLazy("Children", () =>
                {
                    var childIds = content.AllChildIds(hiveManager);
                    return new DynamicContentList(content.Id, hiveManager, membershipService, publicAccessService, new BendyObject(), childIds);
                });
            bendy.AddLazy("Parent", () => content.ParentContent(hiveManager).Bend(hiveManager, membershipService, publicAccessService));
            bendy.AddLazy("Path", () => content.GetPath(hiveManager));
            bendy.AddLazy("PathById", () => content.GetPath(hiveManager));
            bendy.AddLazy("Level", () => content.GetPath(hiveManager).Level);

            //Add lazy url property
            bendy.AddLazy("Url", content.NiceUrl);
            bendy.AddLazy("NiceUrl", content.NiceUrl);
            bendy.AddLazy("NodeTypeAlias", () => content.ContentType.IfNotNull(x => x.Alias, string.Empty));
            bendy.AddLazy("Template", () => content.CurrentTemplate.IfNotNull(x => x.Alias, string.Empty));
            bendy.AddLazy("TemplateId", () => content.CurrentTemplate.IfNotNull(x => x.Id, HiveId.Empty));
            bendy.AddLazy("TemplateFileName", () => content.CurrentTemplate.IfNotNull(x => (string)x.Id.Value, string.Empty));
            bendy.AddLazy("CreateDate", () => content.UtcCreated);
            bendy.AddLazy("UpdateDate", () => content.UtcModified);
            bendy.AddLazy("StatusChangedDate", () => content.UtcStatusChanged);

            dynamicBendy.ContentType = content.ContentType;
            var nodeNameAlias = NodeNameAttributeDefinition.AliasValue.ToRebelAlias(StringAliasCaseType.PascalCase);
            try
            {
                dynamicBendy.Name = bendy[nodeNameAlias].Name;
                dynamicBendy.UrlName = bendy[nodeNameAlias].UrlName;
            }
            catch
            {
                /* Nothing */
            }

            dynamicBendy.Id = content.Id;

            // Add any dynamic registered methods
            BendyObjectExtensionsHelper.ApplyDynamicExtensions<Content>(bendy, dynamicExtensionAssemblies);

            return dynamicBendy;
        }

        private static void AddToBendy(BendyObject bendy, string attribAlias, object value)
        {
            bendy[attribAlias.ToRebelAlias(StringAliasCaseType.PascalCase)] = value;
            bendy[attribAlias.ToRebelAlias(StringAliasCaseType.Unchanged)] = value;
        }
    }
}