using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;
using Umbraco.Framework.Persistence.ModelFirst;
using Umbraco.Framework.Persistence.ModelFirst.Annotations;
using Umbraco.Framework.Linq;
using Umbraco.Framework.Linq.CriteriaGeneration.StructureMetadata;
using Umbraco.Framework.Security.Model.Schemas;

namespace Umbraco.Framework.Security.Model.Entities
{
    [DefaultSchemaForQuerying(SchemaAlias = UserGroupSchema.SchemaAlias)]
    [QueryStructureBinderOfType(typeof(AnnotationQueryStructureBinder))]
    public class UserGroup : CustomTypedEntity<UserGroup>
    {
        public UserGroup()
        {
            this.SetupFromSchema<UserGroupSchema>();
        }

        //public string Name
        //{
        //    get { return Attributes[NodeNameAttributeDefinition.AliasValue].DynamicValue; }
        //    set { Attributes[NodeNameAttributeDefinition.AliasValue].DynamicValue = value; }
        //}

        [AttributeAlias(Alias = NodeNameAttributeDefinition.AliasValue)]
        public string Name
        {
            get { return base.BaseAutoGet(x => x.Name); }
            set { base.BaseAutoSet(x => x.Name, value); }
        }

    }
}
