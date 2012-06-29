using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Security.Model.Entities;

namespace Umbraco.Framework.Security
{
    public interface IMembershipService<TUserType>
        where TUserType : Profile, IMembershipUser, new()
    {
        TUserType Create(TUserType user, out MembershipCreateStatus status);
        void Update(TUserType user);

        bool Delete(HiveId id, bool deleteAllRelatedData = false);

        TUserType GetById(string id, bool userIsOnline = false);
        TUserType GetById(HiveId id, bool userIsOnline = false);

        TUserType GetByProviderUserKey(object providerUserKey, bool userIsOnline = false);

        TUserType GetByUsername(string username, bool userIsOnline = false);

        TUserType GetByProfileId(string profileId, bool userIsOnline = false);
        TUserType GetByProfileId(HiveId profileId, bool userIsOnline = false);

        IEnumerable<TUserType> GetAll();
        IEnumerable<TUserType> GetAll(int pageIndex, int pageSize, out int totalRecords);

        IEnumerable<TUserType> FindByEmail(string emailToMatch);
        IEnumerable<TUserType> FindByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords);

        IEnumerable<TUserType> FindByUsername(string usernameToMatch);
        IEnumerable<TUserType> FindByUsername(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords);

        IEnumerable<TUserType> FindByName(string nameToMatch);
        IEnumerable<TUserType> FindByName(string nameToMatch, int pageIndex, int pageSize, out int totalRecords);

        bool Validate(string username, string password, bool updateLastLoginDate = false);
        bool Unlock(string username);
        int GetNumberOfUsersOnline();

        string GetPassword(string username, string answer);
        bool ChangePassword(string username, string oldPassword, string newPassword);
        bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer);
        string ResetPassword(string username, string answer);

        MembershipProvider MembershipProvider { get; }
    }
}
