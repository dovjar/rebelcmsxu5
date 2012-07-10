using Rebel.Framework.Persistence;
using Rebel.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Rebel.Framework.Persistence.Model.Constants.Schemas;
using Rebel.Framework.Persistence.ModelFirst;
using Rebel.Framework.Persistence.ModelFirst.Annotations;
using Rebel.Framework.Linq;
using Rebel.Framework.Linq.CriteriaGeneration.StructureMetadata;
using Rebel.Framework.Security.Model.Schemas;

namespace Rebel.Framework.Security.Model.Entities
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
