using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.AttributeTypes;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;

namespace Umbraco.Framework.Security.Model.Schemas  
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

            RelationProxies.EnlistParentById(Umbraco.Framework.Persistence.Model.Constants.FixedHiveIds.SystemRoot, FixedRelationTypes.PermissionRelationType);

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
