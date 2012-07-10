using Rebel.Framework.Persistence;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Rebel.Framework.Persistence.Model.Constants.AttributeTypes;
using Rebel.Framework.Persistence.Model.Constants.Schemas;

namespace Rebel.Framework.Security.Model.Schemas  
{
    public class UserGroupSchema : SystemSchema
    {
        public const string SchemaAlias = "system-user-group";

        public UserGroupSchema()
        {
            this.Setup(SchemaAlias, "User Group");
            
            Id = FixedHiveIds.UserGroupSchema;
            SchemaType = FixedSchemaTypes.UserGroup;
            var userGroupDetails = FixedGroupDefinitions.UserGroupDetails;

            RelationProxies.EnlistParentById(Rebel.Framework.Persistence.Model.Constants.FixedHiveIds.SystemRoot, FixedRelationTypes.PermissionRelationType);

            var inBuiltTextType = AttributeTypeRegistry.Current.GetAttributeType(StringAttributeType.AliasValue);

            this.AttributeDefinitions.Add(new AttributeDefinition(NodeNameAttributeDefinition.AliasValue, "Name")
            {
                Id = new HiveId("ug-name".EncodeAsGuid()),
                AttributeType = inBuiltTextType,
                AttributeGroup = userGroupDetails,
                Ordinal = 0,
                Description = "user group name"
            });
        }
    }
}
