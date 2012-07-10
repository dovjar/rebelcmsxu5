using Rebel.Framework.Persistence;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.Schemas;

namespace Rebel.Framework.Security.Model.Schemas
{
    public class MasterMemberProfileSchema : ProfileSchema
    {
        public const string SchemaAlias = "system-master-member-profile";

        public MasterMemberProfileSchema()
        {
            this.Setup(SchemaAlias, "Master Member Profile");

            Id = FixedHiveIds.MasterMemberProfileSchema;
            SchemaType = FixedSchemaTypes.MasterMemberProfile;

            CreatedAttributeDefinitions();
        }

        protected override AttributeGroup DetailsGroup
        {
            get { return FixedGroupDefinitions.UserDetails; }
        }
    }
}
