using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model.BackOffice.Editors;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.TypeMapping;
using Rebel.Hive;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Cms.Web.Mapping
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