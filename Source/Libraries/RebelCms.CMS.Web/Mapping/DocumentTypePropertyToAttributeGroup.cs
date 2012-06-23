using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Model.BackOffice.Editors;
using RebelCms.Framework;
using RebelCms.Framework.Persistence.Model.Attribution.MetaData;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.TypeMapping;
using RebelCms.Hive;
using RebelCms.Hive.RepositoryTypes;

namespace RebelCms.Cms.Web.Mapping
{
    /// <summary>
    /// Maps from a DocumentTypeProperty to an AttributeGroup
    /// </summary>
    internal class DocumentTypePropertyToAttributeGroup : StandardMemberMapper<DocumentTypeProperty, AttributeGroup>
    {
        private readonly bool _returnNullValue;

        public DocumentTypePropertyToAttributeGroup(AbstractFluentMappingEngine currentEngine, MapResolverContext context, bool returnNullValue = false)
            : base(currentEngine, context)
        {
            _returnNullValue = returnNullValue;
        }

        public override AttributeGroup GetValue(DocumentTypeProperty source)
        {
            if (_returnNullValue) return null;

            //if there is no tab specified, then it needs to go on the general tab... 
            if (source.TabId.IsNullValueOrEmpty())
            {
                return FixedGroupDefinitions.GeneralGroup;
            }

            //retreive all of the tabs
            using (var uow = ResolverContext.Hive.OpenReader<IContentStore>())
            {
                var group = uow.Repositories.Schemas.Get<AttributeGroup>(source.TabId) ?? FixedGroupDefinitions.GeneralGroup;
                return group;
            }
        }
    }
}