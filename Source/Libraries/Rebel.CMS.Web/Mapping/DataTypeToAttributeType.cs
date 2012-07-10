using System;
using Rebel.Cms.Web.Model.BackOffice;
using Rebel.Framework;
using Rebel.Framework.Persistence;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants.SerializationTypes;
using Rebel.Framework.TypeMapping;

namespace Rebel.Cms.Web.Mapping
{
    /// <summary>
    /// Maps a DataType or DataTypeEditorModel to an AttributeType
    /// </summary>
    internal class DataTypeToAttributeType<TInput> : TypeMapper<TInput, AttributeType>
        where TInput : CoreEntityModel
    {
        public DataTypeToAttributeType(AbstractFluentMappingEngine engine, Action<TInput, AttributeType> additionalAfterMap = null)
            : base(engine)
        {
            MappingContext
                .CreateUsing(x =>
                    {
                        var typeRegistry = AttributeTypeRegistry.Current;
                        if (typeRegistry != null)
                        {
                            var existing = typeRegistry.TryGetAttributeType(x.Name).Result;
                            if (existing != null) return existing;
                        }
                        return new AttributeType() {Id = x.Id};
                    })
                // Ignore the Id. In the CreateUsing, we either return the AttributeType from the registry with its inferred Id,
                // or we set the Id there from the source.
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.UtcCreated, opt => opt.MapUsing<UtcCreatedMapper>())
                .ForMember(x => x.UtcModified, opt => opt.MapUsing<UtcModifiedMapper>())
                .AfterMap((source, dest) =>
                    {
                        if (string.IsNullOrEmpty(dest.Alias))
                            dest.Alias = source.Name.ToRebelAlias();

                        //TODO: Detect serialization type from DataType
                        dest.SerializationType = new LongStringSerializationType();

                        if (additionalAfterMap != null)
                        {
                            additionalAfterMap(source, dest);
                        }
                    });
        }
    }
}