using Rebel.Cms.Web.Model.BackOffice.Editors;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Versioning;
using Rebel.Framework.TypeMapping;

namespace Rebel.Cms.Web.Mapping
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