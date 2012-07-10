using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model.BackOffice.Editors;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.TypeMapping;
using Rebel.Hive;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Cms.Web.Mapping
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