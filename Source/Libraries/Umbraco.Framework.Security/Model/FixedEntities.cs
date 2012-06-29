using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Framework.Persistence.Model.Constants.Entities;

namespace Umbraco.Framework.Security.Model
{
    public static class FixedEntities
    {
        public static SubContentRoot UserVirtualRoot
        {
            get
            {
                return new SubContentRoot(FixedHiveIds.UserVirtualRoot);
            }
        }

        public static SubContentRoot MemberVirtualRoot
        {
            get
            {
                return new SubContentRoot(FixedHiveIds.MemberVirtualRoot);
            }
        }

        public static SubContentRoot UserProfileVirtualRoot
        {
            get
            {
                return new SubContentRoot(FixedHiveIds.UserProfileVirtualRoot);
            }
        }

        public static SubContentRoot MemberProfileVirtualRoot
        {
            get
            {
                return new SubContentRoot(FixedHiveIds.MemberProfileVirtualRoot);
            }
        }

        public static SubContentRoot UserGroupVirtualRoot
        {
            get
            {
                return new SubContentRoot(FixedHiveIds.UserGroupVirtualRoot);
            }
        }

        public static SubContentRoot MemberGroupVirtualRoot
        {
            get
            {
                return new SubContentRoot(FixedHiveIds.MemberGroupVirtualRoot);
            }
        }
    }
}
