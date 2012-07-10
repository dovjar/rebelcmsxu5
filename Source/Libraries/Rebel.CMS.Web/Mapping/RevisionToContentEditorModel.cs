using Rebel.Cms.Web.Model.BackOffice.Editors;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Versioning;
using Rebel.Framework.TypeMapping;

namespace Rebel.Cms.Web.Mapping
{
    internal class RevisionToContentEditorModel<TContentModel> : TypeMapper<Revision<TypedEntity>, TContentModel>
        where TContentModel : StandardContentEditorModel
    {
        public RevisionToContentEditorModel(AbstractFluentMappingEngine engine)
            : base(engine)
        {
            MappingContext
                .CreateUsing(x =>
                    {
                        var output = MappingContext.Engine.Map<TypedEntity, TContentModel>(x.Item);
                        output.RevisionId = x.MetaData.Id;
                        return output;
                    });
        }
    }
}