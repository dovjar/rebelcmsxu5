using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Rebel.Framework.Persistence.Model.Constants.Entities;
using Rebel.Framework.Security;
using Rebel.Framework.Security.Model.Entities;

namespace Rebel.Cms.Web
{
    public static class MembershipServiceExtensions
    {
        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <typeparam name="TUserType">The type of the user.</typeparam>
        /// <param name="membershipService">The membership service.</param>
        /// <returns></returns>
        public static TUserType GetCurrent<TUserType>(this IMembershipService<TUserType> membershipService)
            where TUserType : Profile, IMembershipUser, new()
        {
            if(HttpContext.Current == null || HttpContext.Current.User == null || !HttpContext.Current.User.Identity.IsAuthenticated)
                return default(TUserType);

            return membershipService.GetByUsername(HttpContext.Current.User.Identity.Name);
        }
    }
}
