using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Framework.Persistence.Model.Constants.Entities;

namespace Rebel.Framework.Security.Model
{
    public static class FixedHiveIds
    {
        public static readonly HiveId UserVirtualRoot = HiveId.ConvertIntToGuid(new Uri("security://users"), null, -5);
        public static readonly HiveId MemberVirtualRoot = HiveId.ConvertIntToGuid(new Uri("security://members"), null, -11);

        public static readonly HiveId UserProfileVirtualRoot = HiveId.ConvertIntToGuid(new Uri("security://user-profiles"), null, -12);
        public static readonly HiveId MemberProfileVirtualRoot = HiveId.ConvertIntToGuid(new Uri("security://member-profiles"), null, -13);

        public static readonly HiveId UserGroupVirtualRoot = HiveId.ConvertIntToGuid(new Uri("security://user-groups"), null, -6);
        public static readonly HiveId MemberGroupVirtualRoot = HiveId.ConvertIntToGuid(new Uri("security://member-groups"), null, -14);

        public static readonly HiveId RTMUserSchema = HiveId.ConvertIntToGuid("system", null, -701);
        public static readonly HiveId MembershipUserSchema = HiveId.ConvertIntToGuid("system", null, -711);
        public static readonly HiveId MasterUserProfileSchema = HiveId.ConvertIntToGuid("system", null, -710);
        public static readonly HiveId MasterMemberProfileSchema = HiveId.ConvertIntToGuid("system", null, -712);

        public static readonly HiveId UserGroupSchema = HiveId.ConvertIntToGuid("system", null, -702);
    }
}
