using RebelCms.Cms.Web.Model.BackOffice.Editors;
using RebelCms.Framework.Persistence.Model.Attribution;
using RebelCms.Framework.TypeMapping;
using EditorModel = RebelCms.Cms.Web.Model.BackOffice.PropertyEditors.EditorModel;

namespace RebelCms.Cms.Web.Mapping
{
    internal class ContentPropertyToTypedAttribute : TypeMapper<ContentProperty, TypedAttribute>
    {
        private readonly bool _ignoreAttributeDef;

        public ContentPropertyToTypedAttribute(AbstractFluentMappingEngine engine, bool ignoreAttributeDef = false)
            : base(engine)
        {
            _ignoreAttributeDef = ignoreAttributeDef;

            MappingContext
                .ForMember(x => x.UtcCreated, opt => opt.MapUsing<UtcCreatedMapper>())
                .ForMember(x => x.UtcModified, opt => opt.MapUsing<UtcModifiedMapper>())
                .ForMember(x => x.AttributeDefinition, opt => opt.MapFrom(x => _ignoreAttributeDef ? null : x.DocTypeProperty))
                .AfterMap((source, dest) =>
                    {
                        var propEditor = ((EditorModel)source.PropertyEditorModel);
                        if (propEditor != null)
                        {
                            dest.Values.Clear();
                            foreach (var i in propEditor.GetSerializedValue())
                            {
                                dest.Values.Add(i.Key, i.Value);
                            }
                        }
                        else
                        {
                            dest.DynamicValue = null;
                        }

                    });
        }
    }
}