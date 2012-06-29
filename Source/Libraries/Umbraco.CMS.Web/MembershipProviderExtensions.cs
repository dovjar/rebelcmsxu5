using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using Umbraco.Cms.Web.Security;

namespace Umbraco.Cms.Web
{
    public static class MembershipProviderExtensions
    {
        public static MembershipUserCollection GetAllUsers(this MembershipProvider provider, out int totalUsers)
        {
            return provider.GetAllUsers(0, Int32.MaxValue, out totalUsers);
        }

        public static MembershipProvider GetUsersMembershipProvider(this MembershipProviderCollection providers)
        {
            return providers["UsersMembershipProvider"];
        }

        public static MembershipProvider GetMembersMembershipProvider(this MembershipProviderCollection providers)
        {
            return providers["MembersMembershipProvider"];
        }

        public static bool IsUsingDefaultUsersMembershipProvider(this MembershipProviderCollection providers)
        {
            var provider = providers.GetUsersMembershipProvider();
            return provider.IsHiveMembershipProvider();
        }

        public static bool IsUsingDefaultMembersMembershipProvider(this MembershipProviderCollection providers)
        {
            var provider = providers.GetMembersMembershipProvider();
            return provider.IsHiveMembershipProvider();
        }

        public static bool IsHiveMembershipProvider(this MembershipProvider provider)
        {
            return provider is AbstractUmbracoMembershipProvider;
        }
    }
}
