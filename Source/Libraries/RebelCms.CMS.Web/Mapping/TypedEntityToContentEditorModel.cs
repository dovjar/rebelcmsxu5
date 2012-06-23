using System;
using System.Collections.Generic;
using System.Linq;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Model.BackOffice.Editors;
using RebelCms.Cms.Web.Model.BackOffice.PropertyEditors;
using RebelCms.Framework;
using RebelCms.Framework.Persistence.Model;
using RebelCms.Framework.Persistence.Model.Attribution.MetaData;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.TypeMapping;

namespace RebelCms.Cms.Web.Mapping
{
    internal class TypedEntityToContentEditorModel<TEntity, TContent> : TypeMapper<TEntity, TContent>
        where TContent : BasicContentEditorModel
        where TEntity : TypedEntity
    {
        private readonly MapResolverContext _resolverContext;

        public TypedEntityToContentEditorModel(
            AbstractFluentMappingEngine engine, 
            MapResolverContext resolverContext,
            Action<TEntity, TContent> additionalAfterMap = null)
            : base(engine)
        {
            _resolverContext = resolverContext;
            MappingContext
                .MapMemberFrom(x => x.DocumentTypeAlias, x => x.EntitySchema.Alias)
                .MapMemberFrom(x => x.DocumentTypeName, x => x.EntitySchema.Name.ToString())
                .MapMemberFrom(x => x.DocumentTypeId, x => x.EntitySchema.Id)
                .MapMemberUsing(x => x.Properties, new TypedEntityToContentProperties<TEntity>(MappingContext.Engine, _resolverContext))
                .AfterMap((source, dest) =>
                    {
                        if (source == null)
                            return;

                        //set the tabs and check if it's a composite schema
                        var compositeSchema = source.EntitySchema as CompositeEntitySchema;
                        IEnumerable<AttributeGroup> attributeGroups = source.EntitySchema.AttributeGroups.ToArray();
                        if (compositeSchema != null) attributeGroups = attributeGroups.Concat(compositeSchema.InheritedAttributeGroups);

                        dest.Tabs = MappingContext.Engine.Map<IEnumerable<AttributeGroup>, HashSet<Tab>>(attributeGroups);

                        var firstParentFound =
                            source.RelationProxies.GetParentRelations(FixedRelationTypes.DefaultRelationType).FirstOrDefault();

                        if (firstParentFound != null && !firstParentFound.Item.SourceId.IsNullValueOrEmpty())
                        {
                            dest.ParentId = firstParentFound.Item.SourceId;
                            dest.OrdinalRelativeToParent = firstParentFound.Item.Ordinal;
                        }

                        //now, update the PropertyEditor context for each property with this item
                        foreach (var p in dest.Properties
                            .Where(p => p.DocTypeProperty.DataType.InternalPropertyEditor != null)
                            .Where(p => p.DocTypeProperty.DataType.InternalPropertyEditor is IContentAwarePropertyEditor))
                        {
                            var contentAwareProp = (IContentAwarePropertyEditor) p.DocTypeProperty.DataType.InternalPropertyEditor;
                            contentAwareProp.SetContentItem(dest);
                            contentAwareProp.SetDocumentType(MappingContext.Engine.Map<EntitySchema, DocumentTypeEditorModel>(source.EntitySchema));
                        }

                        if (additionalAfterMap != null)
                        {
                            additionalAfterMap(source, dest);
                        }
                    });
        }
    }
}