using RebelCms.Cms.Web.Model.BackOffice.Editors;
using RebelCms.Framework.Persistence.Model;
using RebelCms.Framework.Persistence.Model.Versioning;
using RebelCms.Framework.TypeMapping;
using RebelCms.Hive;

namespace RebelCms.Cms.Web.Mapping
{
    internal class EntitySnapshotToContentEditorModel<TContentModel> : TypeMapper<EntitySnapshot<TypedEntity>, TContentModel>
        where TContentModel : StandardContentEditorModel
    {
        public EntitySnapshotToContentEditorModel(AbstractFluentMappingEngine engine)
            : base(engine)
        {

            MappingContext
                .CreateUsing(x => MappingContext.Engine.Map<Revision<TypedEntity>, TContentModel>(x.Revision))
                .AfterMap(
                    (source, dest) =>
                    {
                        dest.UtcPublishedDate = source.PublishedDate();
                    });

        }


    }
}