﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Framework.Security.Model.Schemas;

namespace Rebel.Framework.Security.Model
{
    public static class FixedSchemas
    {
        public static UserGroupSchema UserGroup { get { return new UserGroupSchema(); } }
        public static MembershipUserSchema MembershipUserSchema { get { return new MembershipUserSchema(); } }
        public static MasterUserProfileSchema UserProfileSchema { get { return new MasterUserProfileSchema(); } }
        public static MasterMemberProfileSchema MemberProfileSchema { get { return new MasterMemberProfileSchema(); } }
    }
}
