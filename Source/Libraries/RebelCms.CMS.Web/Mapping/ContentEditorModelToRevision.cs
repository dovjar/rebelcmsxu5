using RebelCms.Cms.Web.Model.BackOffice.Editors;
using RebelCms.Framework.Persistence.Model;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Persistence.Model.Versioning;
using RebelCms.Framework.TypeMapping;

namespace RebelCms.Cms.Web.Mapping
{
    internal class ContentEditorModelToRevision<TContentModel> : TypeMapper<TContentModel, Revision<TypedEntity>>
        where TContentModel : StandardContentEditorModel
    {
        public ContentEditorModelToRevision(AbstractFluentMappingEngine engine)
            : base(engine)
        {
            MappingContext
                .CreateUsing(x => new Revision<TypedEntity>()
                    {
                        Item = MappingContext.Engine.Map<TContentModel, TypedEntity>(x),
                        //TODO: Is this correctly mapped ??
                        MetaData = new RevisionData(x.RevisionId, FixedStatusTypes.Draft)
                    });

        }


    }
}