using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants.SerializationTypes;

namespace Rebel.Framework.Persistence.Model.Constants.AttributeTypes
{
    public class UserGroupUsersAttributeType : AttributeType
    {
        public const string AliasValue = "system-user-group-member-type";

        public UserGroupUsersAttributeType()
            : base(
                AliasValue,
                AliasValue,
                "This type represents the internal UserGroupMemners",
                new StringSerializationType())
        {
            Id = FixedHiveIds.UserGroupMemberAttributeType;
        }
    }
}
