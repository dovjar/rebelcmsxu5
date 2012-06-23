using RebelCms.Cms.Web.Model.BackOffice.Editors;
using RebelCms.Framework.Persistence.Model;
using RebelCms.Framework.Persistence.Model.Versioning;
using RebelCms.Framework.TypeMapping;

namespace RebelCms.Cms.Web.Mapping
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