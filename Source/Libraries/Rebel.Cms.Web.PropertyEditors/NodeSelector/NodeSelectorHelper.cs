using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model;
using Rebel.Framework;
using Rebel.Framework.Context;
using Rebel.Framework.Dynamics;
using Rebel.Framework.Dynamics.Attributes;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Attribution;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Versioning;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Cms.Web.PropertyEditors.NodeSelector
{
    /// <summary>
    /// Extension methods for use in templates to extract the HiveIds selected in the NodeSelector
    /// </summary>
    [DynamicExtensions]
    public static class NodeSelectorHelper
    {
        
        /// <summary>
        /// Returns all Content items selected
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="includeUnpublished">true to include items that are currently unpublished, default is false</param>
        /// <param name="includeRecycled">true to include items that have been recycled, default is false</param>
        /// <returns></returns>
        /// <remarks>
        /// This method will only work if the attribute is configured to use a NodeSelector that has the Content tree selected as its tree type
        /// </remarks>
        [DynamicFieldExtension(CorePluginConstants.NodeSelectorEditorId)]
        public static IEnumerable<Content> SelectedContentEntities(this TypedAttribute attribute, bool includeUnpublished, bool includeRecycled)
        {
            Mandate.ParameterNotNull(attribute, "attribute");

            var nodeSelector = ValidateNodeSelectorPropertyEditor(attribute);
            return GetItemsWithValidation<Content, IContentStore>(
                attribute, nodeSelector, Guid.Parse(FixedTreeIds.NodeSelectorContentTreeId),
                () => "This method can only be used when the attribute is defined by a NodeSelector PropertyEditor that is configured to use the Content tree",
                includeUnpublished, includeRecycled, FixedHiveIds.ContentRecylceBin);
        }
        
        /// <summary>
        /// Returns all Content items selected
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        [DynamicFieldExtension(CorePluginConstants.NodeSelectorEditorId)]
        public static IEnumerable<Content> SelectedContentEntities(this TypedAttribute attribute)
        {
            return attribute.SelectedContentEntities(false, false);
        }

        // BUG: We are returning a Content entity because there is no mapping between TypedEntity and Media entities:
        // http://issues.rebel.org/issue/U5-898

        /// <summary>
        /// Returns all Media items selected
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="includeUnpublished"></param>
        /// <param name="includeRecycled"></param>
        /// <returns></returns>
        [DynamicFieldExtension(CorePluginConstants.NodeSelectorEditorId)]
        public static IEnumerable<Content> SelectedMediaEntities(this TypedAttribute attribute, bool includeUnpublished, bool includeRecycled)
        {
            Mandate.ParameterNotNull(attribute, "attribute");

            var nodeSelector = ValidateNodeSelectorPropertyEditor(attribute);
            return GetItemsWithValidation<Content, IContentStore>(
                attribute, nodeSelector, Guid.Parse(FixedTreeIds.NodeSelectorMediaTreeId),
                () => "This method can only be used when the attribute is defined by a NodeSelector PropertyEditor that is configured to use the Media tree",
                includeUnpublished, includeRecycled, FixedHiveIds.ContentRecylceBin);
        }

        /// <summary>
        /// Returns all Media items selected
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        [DynamicFieldExtension(CorePluginConstants.NodeSelectorEditorId)]
        public static IEnumerable<Content> SelectedMediaEntities(this TypedAttribute attribute)
        {
            return attribute.SelectedMediaEntities(false, false);
        }

        /// <summary>
        /// Returns all Content selected as dynamic items
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="includeUnpublished"></param>
        /// <param name="includeRecycled"></param>
        /// <returns></returns>
        [DynamicFieldExtension(CorePluginConstants.NodeSelectorEditorId)]
        public static IEnumerable<dynamic> SelectedContent(this TypedAttribute attribute, bool includeUnpublished, bool includeRecycled)
        {
            return attribute.SelectedContentEntities(includeUnpublished, includeRecycled)
                .Select(x => x.AsDynamic());
        }

        /// <summary>
        /// Returns all Content selected as dynamic items
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        [DynamicFieldExtension(CorePluginConstants.NodeSelectorEditorId)]
        public static IEnumerable<dynamic> SelectedContent(this TypedAttribute attribute)
        {
            return attribute.SelectedContent(false, false);
        }

        /// <summary>
        /// Returns all Content selected as dynamic items
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="includeUnpublished"></param>
        /// <param name="includeRecycled"></param>
        /// <returns></returns>
        [DynamicFieldExtension(CorePluginConstants.NodeSelectorEditorId)]
        public static IEnumerable<dynamic> SelectedMedia(this TypedAttribute attribute, bool includeUnpublished, bool includeRecycled)
        {
            //BUG: we are returning content currently since there is no AsDynamic method for Media.
            // this should still work but may cause problems if people try to access content specific stuff dynamically.
            // http://issues.rebel.org/issue/U5-959            
            // Also, because of this we cannot validate the media tree type!!

            return GetItems<Content, IContentStore>(attribute.SelectedNodeIds().ToArray(), includeUnpublished, includeRecycled, FixedHiveIds.MediaRecylceBin)
                .AsDynamic();
        }

        /// <summary>
        /// Returns all Content selected as dynamic items
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        [DynamicFieldExtension(CorePluginConstants.NodeSelectorEditorId)]
        public static IEnumerable<dynamic> SelectedMedia(this TypedAttribute attribute)
        {
            return attribute.SelectedMedia(false, false);
        }

        /// <summary>
        /// Returns the list of HiveIds selected with the NodeSelector
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        /// <remarks>
        /// This method will throw an exception if the attribute is not backed by a PropertyEditor of type NodeSelector
        /// </remarks>
        [DynamicFieldExtension(CorePluginConstants.NodeSelectorEditorId)]
        public static IEnumerable<HiveId> SelectedNodeIds(this TypedAttribute attribute)
        {
            Mandate.ParameterNotNull(attribute, "attribute");

            ValidateNodeSelectorPropertyEditor(attribute);

            return attribute.Values
                //need to ensure they are sorted!
                .OrderBy(x =>
                {
                    var index = x.Key.Substring(3, x.Key.Length - 3);
                    int i;
                    return int.TryParse(index, out i) ? i : 0;
                })
                .Select(x =>
                {
                    //ensure the value is parsable, this shouldn't happen but we'll be sure to check.
                    var hiveId = HiveId.TryParse(x.Value.ToString());
                    return hiveId.Success ? hiveId.Result : HiveId.Empty;
                })
                .Where(x => !x.IsNullValueOrEmpty()); //if it didn't parse, don't return it
        }
        
        /// <summary>
        /// Returns the items for either the requested Media or Content
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TStore"></typeparam>
        /// <param name="attribute"></param>
        /// <param name="nodeSelector"></param>
        /// <param name="treeId"></param>
        /// <param name="errMsg"></param>
        /// <param name="includeUnpublished"></param>
        /// <param name="includeRecycled"></param>
        /// <param name="recycleBinId"></param>
        /// <returns></returns>
        private static IEnumerable<T> GetItemsWithValidation<T, TStore>(
            TypedAttribute attribute, 
            NodeSelectorEditor nodeSelector, 
            Guid treeId, 
            Func<string> errMsg, 
            bool includeUnpublished, 
            bool includeRecycled,
            HiveId recycleBinId)
            where T: TypedEntity 
            where TStore : class, IProviderTypeFilter
        {
            var preVals = nodeSelector.CreatePreValueEditorModel();
            preVals.SetModelValues(attribute.AttributeDefinition.AttributeType.RenderTypeProviderConfig);
            //now we can check if it is set to a content tree
            if (preVals.SelectedTree != treeId)
                throw new InvalidOperationException(errMsg());

            return GetItems<T, TStore>(attribute.SelectedNodeIds().ToArray(), includeUnpublished, includeRecycled, recycleBinId);
        }

        /// <summary>
        /// Returns the items for either the requested Media or Content
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TStore"></typeparam>
        /// <param name="selectedIds"></param>
        /// <param name="includeUnpublished"></param>
        /// <param name="includeRecycled"></param>
        /// <param name="recycleBinId"></param>
        /// <returns></returns>
        private static IEnumerable<T> GetItems<T, TStore>(
            HiveId[] selectedIds,
            bool includeUnpublished,
            bool includeRecycled,
            HiveId recycleBinId)
            where T : TypedEntity
            where TStore : class, IProviderTypeFilter
        {
            var nodeIds = selectedIds;
            var appCtx = DependencyResolver.Current.GetService<IRebelApplicationContext>();
            using (var uow = appCtx.Hive.GetReader<TStore>().CreateReadonly())
            {
                var items = uow.Repositories.Revisions.GetLatestRevisions<TypedEntity>(
                    false,
                    !includeUnpublished ? FixedStatusTypes.Published : null,
                    nodeIds)
                    .WhereNotNull()
                    .Select(x => appCtx.FrameworkContext.TypeMappers.Map<T>(x.Item))
                    .ToArray();

                if (!includeRecycled)
                {
                    items = items.Where(x => !x.AllAncestorIds().Contains(recycleBinId, new HiveIdComparer(true))).ToArray();
                }

                return items;
            }
        }

        /// <summary>
        /// Ensures that the attribute is defined by a NodeSelector
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        private static NodeSelectorEditor ValidateNodeSelectorPropertyEditor(TypedAttribute attribute)
        {
            Action throwException = () => { throw new NotSupportedException("This extension method can only be used with an attribute that is defined by a property editor of type NodeSelector"); };
            
            var propEditorFactory = DependencyResolver.Current.GetService<IPropertyEditorFactory>();
            var propEditor = propEditorFactory.GetPropertyEditor(attribute.AttributeDefinition.AttributeType.RenderTypeProvider);
            if (propEditor == null)
                throwException();
            var nodeSelector = propEditor.Value as NodeSelectorEditor;
            if (nodeSelector == null)
                throwException();
            return nodeSelector;
        }
    }
}