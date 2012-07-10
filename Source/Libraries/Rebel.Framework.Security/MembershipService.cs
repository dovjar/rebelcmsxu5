using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using Rebel.Framework.Context;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Associations;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.Entities;
using Rebel.Framework.Persistence.Model.Constants.Schemas;
using Rebel.Framework.Security.Configuration;
using Rebel.Framework.Security.Model.Entities;
using Rebel.Hive;
using Rebel.Hive.RepositoryTypes;
using Rebel.Framework;
using Rebel.Framework.Persistence;

namespace Rebel.Framework.Security
{
    public class MembershipService<TUserType, TProfileType> : IMembershipService<TUserType>
        where TUserType : Profile, IMembershipUser, new()
        where TProfileType : Profile, new()
    {
        private readonly IFrameworkContext _frameworkContext;
        private readonly IHiveManager _hiveManager;
        private readonly string _profileProviderMappingRoot;
        private readonly string _groupsProviderMappingRoot;
        private readonly HiveId _profileVirtualRootId;
        private readonly MembershipProvider _membershipProvider;
        private readonly IEnumerable<MembershipProviderElement> _membershipProvidersConfig;

        public MembershipProvider MembershipProvider
        {
            get { return _membershipProvider; }
        }

        public MembershipProviderElement MembershipProviderConfig
        {
            get { return _membershipProvidersConfig.SingleOrDefault(x => x.Type == MembershipProvider.GetType()); }
        }

        public MembershipService(
            IFrameworkContext context,
            IHiveManager hive,
            string profileProviderMappingRoot,
            string groupsProviderMappingRoot,
            HiveId profileVirtualRootId,
            MembershipProvider membershipProvider,
            IEnumerable<MembershipProviderElement> membershipProvidersConfig)
        {
            _frameworkContext = context;
            _hiveManager = hive;
            _membershipProvider = membershipProvider;

            _profileProviderMappingRoot = profileProviderMappingRoot;
            _profileVirtualRootId = profileVirtualRootId;

            _groupsProviderMappingRoot = groupsProviderMappingRoot;

            _membershipProvidersConfig = membershipProvidersConfig;
        }

        public TUserType Create(TUserType user, out MembershipCreateStatus status)
        {
            //TODO: Could do with making this transactional somehow?
            
            // Create the uer
            var membershipUser = MembershipProvider.CreateUser(user.Username,
                user.Password,
                user.Email,
                user.PasswordQuestion,
                user.PasswordAnswer,
                user.IsApproved,
                user.ProviderUserKey,
                out status);

            if (status != MembershipCreateStatus.Success)
                return user;

            user.ProviderUserKey = membershipUser.ProviderUserKey;

            // Create the profile
            var profile = new TProfileType();

            // Set up the new profile with the same schema as the user so it has any inherited properties
            profile.SetupFromSchema(user.EntitySchema);

            if (MembershipProviderConfig != null)
                profile.SetProviderUserKeyType(MembershipProviderConfig.ProviderUserKeyType);

            // Map the user to the profile now
            _frameworkContext.TypeMappers.Map(user, profile);

            var hive = _hiveManager.GetWriter<ISecurityStore>(new Uri(_profileProviderMappingRoot));
            using (var uow = hive.Create())
            {
                uow.Repositories.AddOrUpdate(profile);

                uow.Repositories.AddRelation(_profileVirtualRootId, profile.Id, FixedRelationTypes.DefaultRelationType, 0);

                uow.Complete();
            }

            // Assign user to groups
            var groupHive = _hiveManager.GetWriter<ISecurityStore>(new Uri(_groupsProviderMappingRoot));
            using (var uow = groupHive.Create())
            {
                if (user.Groups != null)
                {
                    foreach (var groupId in user.Groups)
                    {
                        uow.Repositories.AddRelation(new Relation(FixedRelationTypes.UserGroupRelationType, groupId, profile.Id));
                    }
                }

                uow.Complete();
            }

            //NOTE: This causes another hit to the db but I do it to ensure all profile props are set
            return ConvertToMergedEntity(membershipUser);
        }

        public void Update(TUserType user)
        {
            // Try to fetch user
            var membershipUser = MembershipProvider.GetUser(user.ProviderUserKey, false);
            
            if (membershipUser == null)
                return;

            // Update user
            _frameworkContext.TypeMappers.Map(user, membershipUser);

            MembershipProvider.UpdateUser(membershipUser);

            // Update password
            if(!user.Password.IsNullOrWhiteSpace())
            {
                var tempPassword = MembershipProvider.ResetPassword(user.Username, user.PasswordAnswer);
                MembershipProvider.ChangePassword(user.Username, tempPassword, user.Password);
            }

            // Update profile
            var profile = GetProfileForUser(membershipUser.ProviderUserKey);

            _frameworkContext.TypeMappers.Map(user, profile);

            var hive = _hiveManager.GetWriter<ISecurityStore>(new Uri(_profileProviderMappingRoot));
            using (var uow = hive.Create())
            {
                uow.Repositories.AddOrUpdate(profile); 

                uow.Complete();
            }

            // Update groups
            var groupHive = _hiveManager.GetWriter<ISecurityStore>(new Uri(_groupsProviderMappingRoot));
            using (var uow = groupHive.Create())
            {
                // Remove any removed user groups
                foreach (var relation in uow.Repositories.GetParentRelations(profile.Id, FixedRelationTypes.UserGroupRelationType)
                    .Where(x => !user.Groups.Contains(x.SourceId)))
                {
                    uow.Repositories.RemoveRelation(relation);
                }

                // Add any new user groups
                var existingRelations = uow.Repositories.GetParentRelations(profile.Id, FixedRelationTypes.UserGroupRelationType).Select(x => x.SourceId).ToArray();
                foreach (var groupId in user.Groups.Where(x => !existingRelations.Contains(x)))
                {
                    uow.Repositories.AddRelation(new Relation(FixedRelationTypes.UserGroupRelationType, groupId, profile.Id));
                }

                uow.Complete();
            }
        }

        public bool Delete(HiveId id, bool deleteAllRelatedData = false)
        {
            var membershipUser = MembershipProvider.GetUser(id.Value.Value.ToString(), false);
            return membershipUser != null && DoDeleteUser(membershipUser, deleteAllRelatedData);
        }

        public TUserType GetById(string id, bool userIsOnline = false)
        {
            return GetById(HiveId.Parse(id), userIsOnline);
        }

        public TUserType GetById(HiveId id, bool userIsOnline = false)
        {
            return GetByUsername(id.Value.Value.ToString(), userIsOnline);
        }

        public TUserType GetByProviderUserKey(object providerUserKey, bool userIsOnline = false)
        {
            var membershipUser = MembershipProvider.GetUser(providerUserKey, userIsOnline);
            return membershipUser != null ? ConvertToMergedEntity(membershipUser) : null;
        }

        public TUserType GetByUsername(string username, bool userIsOnline = false)
        {
            var membershipUser = MembershipProvider.GetUser(username, userIsOnline);
            return membershipUser != null ? ConvertToMergedEntity(membershipUser) : null;
        }

        public TUserType GetByProfileId(string profileId, bool userIsOnline = false)
        {
            return GetByProfileId(HiveId.Parse(profileId), userIsOnline);
        }

        public TUserType GetByProfileId(HiveId profileId, bool userIsOnline = false)
        {
            var hive = _hiveManager.GetReader<ISecurityStore>(new Uri(_profileProviderMappingRoot));
            using (var uow = hive.CreateReadonly())
            {
                var profile = uow.Repositories.Get<TProfileType>(profileId);
                if (profile == null)
                    return null;

                if (MembershipProviderConfig != null)
                    profile.SetProviderUserKeyType(MembershipProviderConfig.ProviderUserKeyType);

                return GetByProviderUserKey(profile.ProviderUserKey, userIsOnline);
            }
        }

        public IEnumerable<TUserType> GetAll()
        {
            int totalRecords;
            return GetAll(0, Int32.MaxValue, out totalRecords);
        }

        public IEnumerable<TUserType> GetAll(int pageIndex, int pageSize, out int totalRecords)
        {
            var membershipUsers = MembershipProvider.GetAllUsers(pageIndex, pageSize, out totalRecords)
                .Cast<global::System.Web.Security.MembershipUser>();

            //TODO: This needs to be better, as currently, it'll hit the db for every profile to load
            return membershipUsers.Select(ConvertToMergedEntity);
        }

        public IEnumerable<TUserType> FindByEmail(string emailToMatch)
        {
            int totalRecords;
            return FindByEmail(emailToMatch, 0, Int32.MaxValue, out totalRecords);
        }

        public IEnumerable<TUserType> FindByEmail(string emailToMatch, int pageIndex, int pageSize,
                                                     out int totalRecords)
        {
            if (MembershipProviderConfig != null)
                emailToMatch = emailToMatch.Replace("*", MembershipProviderConfig.Wildcard);

            var membershipUsers = MembershipProvider.FindUsersByEmail(emailToMatch, pageIndex, pageSize,
                                                     out totalRecords)
                .Cast<global::System.Web.Security.MembershipUser>();

            //TODO: This needs to be better, as currently, it'll hit the db for every profile to load
            return membershipUsers.Select(ConvertToMergedEntity);
        }

        public IEnumerable<TUserType> FindByUsername(string usernameToMatch)
        {
            int totalRecords;
            return FindByUsername(usernameToMatch, 0, Int32.MaxValue, out totalRecords);
        }

        public IEnumerable<TUserType> FindByUsername(string usernameToMatch, int pageIndex, int pageSize,
                                                        out int totalRecords)
        {
            if (MembershipProviderConfig != null)
                usernameToMatch = usernameToMatch.Replace("*", MembershipProviderConfig.Wildcard);

            var membershipUsers = MembershipProvider.FindUsersByName(usernameToMatch, pageIndex, pageSize,
                                                     out totalRecords)
                .Cast<global::System.Web.Security.MembershipUser>();

            //TODO: This needs to be better, as currently, it'll hit the db for every profile to load
            return membershipUsers.Select(ConvertToMergedEntity);
        }

        public IEnumerable<TUserType> FindByName(string nameToMatch)
        {
            int totalRecords;
            return FindByName(nameToMatch, 0, Int32.MaxValue, out totalRecords);
        }

        public IEnumerable<TUserType> FindByName(string nameToMatch, int pageIndex, int pageSize,
                                                    out int totalRecords)
        {
            if (MembershipProviderConfig != null)
                nameToMatch = nameToMatch.Replace("*", MembershipProviderConfig.Wildcard);

            //TODO: This actually needs to check the profile name (could get expensive)
            var membershipUsers = MembershipProvider.FindUsersByName(nameToMatch, pageIndex, pageSize,
                                                     out totalRecords)
                .Cast<global::System.Web.Security.MembershipUser>();

            //TODO: This needs to be better, as currently, it'll hit the db for every profile to load
            return membershipUsers.Select(ConvertToMergedEntity);
        }

        public bool Validate(string username, string password, bool updateLastLoginDate = false)
        {
            var valid = MembershipProvider.ValidateUser(username, password);

            if(valid && updateLastLoginDate)
            {
                var member = MembershipProvider.GetUser(username, true);
                member.LastLoginDate = DateTime.UtcNow;
                MembershipProvider.UpdateUser(member);
            }

            return valid;
        }

        public bool Unlock(string username)
        {
            return MembershipProvider.UnlockUser(username);
        }

        public int GetNumberOfUsersOnline()
        {
            return MembershipProvider.GetNumberOfUsersOnline();
        }

        public string GetPassword(string username, string answer)
        {
            return MembershipProvider.GetPassword(username, answer);
        }

        public bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            return MembershipProvider.ChangePassword(username, oldPassword, newPassword);
        }

        public bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion,
                                                    string newPasswordAnswer)
        {
            return MembershipProvider.ChangePasswordQuestionAndAnswer(username, password, newPasswordQuestion,
                                                                      newPasswordAnswer);
        }

        public string ResetPassword(string username, string answer)
        {
            return MembershipProvider.ResetPassword(username, answer);
        }

        protected bool DoDeleteUser(System.Web.Security.MembershipUser user, bool deleteAllRelatedData)
        {
            if (user != null && deleteAllRelatedData)
            {
                var profile = GetProfileForUser(user.ProviderUserKey);

                if (profile != null && !profile.Id.IsNullValueOrEmpty())
                {
                    var hive = _hiveManager.GetWriter<ISecurityStore>(new Uri(_profileProviderMappingRoot));
                    using (var uow = hive.Create())
                    {
                        uow.Repositories.Delete<TypedEntity>(profile.Id);
                        uow.Complete();
                    }
                }

                //TODO: Delete user group relations
            }

            return MembershipProvider.DeleteUser(user.UserName, deleteAllRelatedData);
        }

        protected TUserType ConvertToMergedEntity(System.Web.Security.MembershipUser user)
        {
            var mappedUser = new TUserType();

            _frameworkContext.TypeMappers.Map<MembershipUser, IMembershipUser>(user, mappedUser);

            // Get and merge profile
            var profile = GetProfileForUser(user.ProviderUserKey);

            _frameworkContext.TypeMappers.Map(profile, mappedUser);

            // Get and merge groups
            var groupHive = _hiveManager.GetReader<ISecurityStore>(new Uri(_groupsProviderMappingRoot));
            using (var uow = groupHive.CreateReadonly())
            {
                mappedUser.Groups = uow.Repositories.GetParentRelations(mappedUser.ProfileId, FixedRelationTypes.UserGroupRelationType)
                    .Select(x => x.SourceId).ToArray();
            }

            return mappedUser;
        }

        protected TProfileType GetProfileForUser(object providerUserKey)
        {
            var hive = _hiveManager.GetReader<ISecurityStore>(new Uri(_profileProviderMappingRoot));
            using (var uow = hive.CreateReadonly())
            {
                var userKeyString = providerUserKey.ToString();
                var profile =
                    uow.Repositories
                        .Query<TProfileType>()
                        .WithParentIds(_profileVirtualRootId)
                        .FirstOrDefault(x => x.ProviderUserKey == userKeyString);

                if (profile != null && profile.ProviderUserKey == null)
                {
                    // TODO: Workaround for a bug in serialization used by caching - 
                    // EntitySchema even if CompositeSchema does not get deserialized as Composite,
                    // therefore inherited properties (such as ProviderUserKey) come out of cache null
                    // This causes the next codeblock below to assume it needs to reset attribs
                    // based on the master (rather than inheriting) schema. So, go back and get the profile
                    // by Id without hitting cache
                    profile = uow.Repositories.Get<TProfileType>(profile.Id);
                }

                if(profile == null)
                {
                    profile = new TProfileType();
                    profile.RelationProxies.EnlistParentById(_profileVirtualRootId, FixedRelationTypes.DefaultRelationType);

                    var schema = uow.Repositories.Schemas.Get<EntitySchema>(profile.EntitySchema.Id);
                    if(schema != null)
                    {
                        profile.SetupFromSchema(schema);
                    }
                }
                else
                {
                    if (profile.EntitySchema != null)
                        profile.SetupFromSchema();
                }

                if (MembershipProviderConfig != null)
                    profile.SetProviderUserKeyType(MembershipProviderConfig.ProviderUserKeyType);

                profile.ProviderUserKey = providerUserKey;

                return profile;
            }
        }
    }
}
