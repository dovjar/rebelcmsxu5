using Rebel.Cms.Web.Model.BackOffice.Editors;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Versioning;
using Rebel.Framework.TypeMapping;
using Rebel.Hive;

namespace Rebel.Cms.Web.Mapping
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