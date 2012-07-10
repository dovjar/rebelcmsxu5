using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using Rebel.Cms.Web.Context;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.Entities;
using Rebel.Framework.Security;
using Rebel.Framework.Security.Model.Entities;
using Rebel.Hive;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Cms.Web.Security
{
    public abstract class AbstractRebelRoleProvider<TUserType> : RoleProvider
        where TUserType : Profile, IMembershipUser, new()
    {
        /// <summary>
        /// Gets or sets the name of the application to store and retrieve role information for.
        /// </summary>
        /// <returns>The name of the application to store and retrieve role information for.</returns>
        public override string ApplicationName { get; set; }

        /// <summary>
        /// Gets or sets the app context.
        /// </summary>
        /// <value>
        /// The app context.
        /// </value>
        public IRebelApplicationContext AppContext { get; set; }

        /// <summary>
        /// Gets or sets the membership service.
        /// </summary>
        /// <value>
        /// The membership service.
        /// </value>
        public IMembershipService<TUserType> MembershipService { get; set; }

        /// <summary>
        /// Gets the hive URI.
        /// </summary>
        public abstract Uri HiveUri { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractRebelRoleProvider"/> class.
        /// </summary>
        protected AbstractRebelRoleProvider()
        {
            AppContext = DependencyResolver.Current.GetService<IRebelApplicationContext>();
            MembershipService = DependencyResolver.Current.GetService<IMembershipService<TUserType>>();
        }

        /// <summary>
        /// Gets a value indicating whether the specified user is in the specified role for the configured applicationName.
        /// </summary>
        /// <param name="username">The user name to search for.</param>
        /// <param name="roleName">The role to search in.</param>
        /// <returns>
        /// true if the specified user is in the specified role for the configured applicationName; otherwise, false.
        /// </returns>
        public override bool IsUserInRole(string username, string roleName)
        {
            return GetRolesForUser(username).Contains(roleName);
        }

        /// <summary>
        /// Gets a list of the roles that a specified user is in for the configured applicationName.
        /// </summary>
        /// <param name="username">The user to return a list of roles for.</param>
        /// <returns>
        /// A string array containing the names of all the roles that the specified user is in for the configured applicationName.
        /// </returns>
        public override string[] GetRolesForUser(string username)
        {
            var member = MembershipService.GetByUsername(username, false);

            if(member == null)
                return new string[0];

            using(var uow = AppContext.Hive.OpenReader<ISecurityStore>())
            {
                var userGroups = uow.Repositories.Get<UserGroup>(true, member.Groups.ToArray());
                return userGroups.Select(x => x.Name).ToArray();
            }
        }

        #region Unimplemented Members

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }
        
        #endregion
    }
}
