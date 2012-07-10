using System;
using System.Collections.Generic;
using Rebel.Framework.Persistence;
using Rebel.Framework.Persistence.Model.Constants.Entities;
using Rebel.Framework.Persistence.Model.Constants.Schemas;
using Rebel.Framework.Security.Model.Schemas;

namespace Rebel.Framework.Security.Model.Entities
{
    /// <summary>
    /// A User entity
    /// </summary>
    public class User : UserProfile, IMembershipUser
    {
        public User()
        {
            this.SetupFromSchema<MasterUserProfileSchema>();
        }

        #region MembershipUser Properties

        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordQuestion { get; set; }
        public string PasswordAnswer { get; set; }
        public string Comments { get; set; }
        public bool IsApproved { get; set; }
        public bool IsOnline { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        public DateTimeOffset LastLoginDate { get; set; }
        public DateTimeOffset LastActivityDate { get; set; }
        public DateTimeOffset LastPasswordChangeDate { get; set; }
        public DateTimeOffset LastLockoutDate { get; set; }

        public HiveId ProfileId { get; set; }
        public IEnumerable<HiveId> Groups { get; set; }

        #endregion
    }
}