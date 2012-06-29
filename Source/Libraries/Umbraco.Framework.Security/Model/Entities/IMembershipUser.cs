using System;
using System.Collections.Generic;

namespace Umbraco.Framework.Security.Model.Entities
{
    public interface IMembershipUser : IMembershipUserId
    {
        HiveId Id { get; set; }
        string Username { get; set; }
        string Email { get; set; }
        string Password { get; set; }
        string PasswordQuestion { get; set; }
        string PasswordAnswer { get; set; }
        string Comments { get; set; }
        bool IsApproved { get; set; }
        bool IsOnline { get; set; }
        bool IsLockedOut { get; set; }
        DateTimeOffset CreationDate { get; set; }
        DateTimeOffset LastLoginDate { get; set; }
        DateTimeOffset LastActivityDate { get; set; }
        DateTimeOffset LastPasswordChangeDate { get; set; }
        DateTimeOffset LastLockoutDate { get; set; }

        HiveId ProfileId { get; set; }
        IEnumerable<HiveId> Groups { get; set; }
    }
}
