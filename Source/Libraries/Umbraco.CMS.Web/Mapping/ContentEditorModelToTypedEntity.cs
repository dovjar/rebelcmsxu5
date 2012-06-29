using System.Linq;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Framework;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.TypeMapping;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Mapping
{
    using Umbraco.Framework.Persistence.Model.Attribution.MetaData;

    using global::System;

    /// <summary>
    /// Maps any type of BasicContentEditorModel to any type of TypedEntity
    /// </summary>
    /// <typeparam name="TContentModel">The type of the content model.</typeparam>
    /// <typeparam name="TTypedEntity">The type of the typed entity.</typeparam>
    internal class ContentEditorModelToTypedEntity<TContentModel, TTypedEntity> : TypeMapper<TContentModel, TTypedEntity>
        where TContentModel : BasicContentEditorModel
        where TTypedEntity : TypedEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentEditorModelToTypedEntity&lt;TContentModel, TTypedEntity&gt;"/> class.
        /// </summary>
        /// <param name="engine">The engine.</param>
        /// <param name="resolverContext">The resolver context.</param>
        /// <param name="ignoreAttributeAliases">The attribute aliases to ignore during attribute mapping</param>
        public ContentEditorModelToTypedEntity(AbstractFluentMappingEngine engine, MapResolverContext resolverContext, params string[] ignoreAttributeAliases)
            : base(engine)
        {
            MappingContext
                .ForMember(x => x.UtcCreated, opt => opt.MapUsing<UtcCreatedMapper>())
                .ForMember(x => x.UtcModified, opt => opt.MapUsing<UtcModifiedMapper>())
                .IgnoreMember(x => x.RelationProxies)
                .MapMemberUsing(x => x.EntitySchema, new ContentEditorModelToEntitySchema<TContentModel>(MappingContext.Engine, resolverContext))
                .AfterMap((source, dest) =>
                    {
                        if (!source.ParentId.IsNullValueOrEmpty())
                        {
                            var ordinalRelativeToParent = 0;

                            using (var uow = resolverContext.Hive.OpenReader<IContentStore>(source.ParentId.ToUri()))
                            {
                                var siblingRelations = uow.Repositories.GetChildRelations(source.ParentId, FixedRelationTypes.DefaultRelationType)
                                    .OrderBy(x => x.Ordinal)
                                    .ToArray();

                                var myRelation = siblingRelations.SingleOrDefault(x => x.DestinationId.Value == source.Id.Value);
                                if (myRelation != null)
                                {
                                    ordinalRelativeToParent = myRelation.Ordinal;
                                }
                                else
                                {
                                    ordinalRelativeToParent = siblingRelations.Max(x => x.Ordinal) + 1;
                                }
                            }

                            // Enlist the specified parent
                            dest.RelationProxies.EnlistParentById(source.ParentId, FixedRelationTypes.DefaultRelationType, ordinalRelativeToParent);
                        }

                        //add or update all properties in the model.
                        //We need to manually create a mapper for this operation in order to keep the object graph consistent
                        //with the correct instances of objects. This mapper will ignore mapping the AttributeDef on the TypedAttribute
                        //so that we can manually assign it from our EntitySchema.
                        var compositeSchema = dest.EntitySchema as CompositeEntitySchema;
                        var propertyMapper = new ContentPropertyToTypedAttribute(MappingContext.Engine, true);
                        var contentProperties = source.Properties.Where(x => !ignoreAttributeAliases.Contains(x.Alias)).ToArray();
                        foreach (var contentProperty in contentProperties)
                        {
                            var typedAttribute = propertyMapper.Map(contentProperty);
                            //now we need to manually assign the same attribute definition instance from our already mapped schema.
                            var localProperty = contentProperty;
                            var localPropertyId = localProperty.DocTypePropertyId;
                            if (localPropertyId == HiveId.Empty)
                                throw new Exception("Catastrophic failure: The content property to be mapped does not have an Id against the document type. Alias: {0}".InvariantFormat(localProperty.Alias));
                            Func<AttributeDefinition, bool> findAttrib = x => x.Id.Value == localPropertyId.Value;
                            var attDef = dest.EntitySchema.AttributeDefinitions.SingleOrDefault(findAttrib);

                            if (compositeSchema != null && attDef == null) attDef = compositeSchema.AllAttributeDefinitions.Single(findAttrib);

                            typedAttribute.AttributeDefinition = attDef;

                            dest.Attributes.SetValueOrAdd(typedAttribute);
                        }
                        //now we need to remove any properties that don't exist in the model, excluding the 'special' internal fields
                        var allAliases = source.Properties.Select(x => x.Alias).ToArray();
                        var toRemove = dest.Attributes.Where(x => !allAliases.Contains(x.AttributeDefinition.Alias))
                            .Select(x => x.Id).ToArray();
                        dest.Attributes.RemoveAll(x => toRemove.Contains(x.Id));


                    });
        }
    }
}