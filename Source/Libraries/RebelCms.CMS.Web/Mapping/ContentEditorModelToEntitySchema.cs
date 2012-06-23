using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Model.BackOffice.Editors;
using RebelCms.Framework;
using RebelCms.Framework.Persistence.Model.Attribution.MetaData;
using RebelCms.Framework.TypeMapping;
using RebelCms.Hive;
using RebelCms.Hive.RepositoryTypes;

namespace RebelCms.Cms.Web.Mapping
{
    internal class ContentEditorModelToEntitySchema<TInput> : StandardMemberMapper<TInput, EntitySchema>
         where TInput : BasicContentEditorModel
    {
        public ContentEditorModelToEntitySchema(AbstractFluentMappingEngine currentEngine, MapResolverContext context)
            : base(currentEngine, context)
        {
        }

        public override EntitySchema GetValue(TInput source)
        {
            if (source.DocumentTypeId.IsNullValueOrEmpty()) return null;

            using (var uow = ResolverContext.Hive.OpenReader<IContentStore>())
            {
                var entity = uow.Repositories.Schemas.GetComposite<EntitySchema>(source.DocumentTypeId);
                return entity;
            }
        }
    }
}