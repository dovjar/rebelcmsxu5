using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using Rebel.Framework.Localization;

namespace Rebel.Cms.Web
{
    public static class MembershipCreateStatusExtensions
    {
        public static string Localize(this MembershipCreateStatus status)
        {
            return ("MembershipCreateStatus." + status.ToString()).Localize();
        }
    }
}
