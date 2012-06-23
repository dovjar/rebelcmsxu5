using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RebelCms.Framework.Persistence.Model.Attribution.MetaData;
using RebelCms.Framework.Persistence.Model.Constants.SerializationTypes;

namespace RebelCms.Framework.Persistence.Model.Constants.AttributeTypes
{
    public class UserGroupUsersAttributeType : AttributeType
    {
        public const string AliasValue = "system-user-group-member-type";

        internal UserGroupUsersAttributeType()
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
