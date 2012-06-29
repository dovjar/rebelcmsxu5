using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations._Revised;
using Umbraco.Framework.Security.Model.Entities;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Framework.Security.Mapping
{
    public class SecurityModelMapper : AbstractFluentMappingEngine
    {
        public SecurityModelMapper(IFrameworkContext frameworkContext) 
            : base(frameworkContext)
        { }

        public override void ConfigureMappings()
        {
            #region MembershipUser -> IMembershipUser

            this.CreateMap<global::System.Web.Security.MembershipUser, IMembershipUser>()
                .ForMember(x => x.Id, x => x.MapFrom(y => y.UserName.IsNullOrWhiteSpace() ? HiveId.Empty : new HiveId(new Uri("security://"), "", new HiveIdValue(y.UserName))))
                .ForMember(x => x.Username, x => x.MapFrom(y => y.UserName))
                .ForMember(x => x.LastPasswordChangeDate, x => x.MapFrom(y => y.LastPasswordChangedDate));

            #endregion

            #region IMembershipUser -> MembershipUser

            this.CreateMap<IMembershipUser, global::System.Web.Security.MembershipUser>(true)
                .CreateUsing(x => new global::System.Web.Security.MembershipUser("",
                        x.Username,
                        x.ProviderUserKey,
                        x.Email,
                        x.PasswordQuestion,
                        x.Comments,
                        x.IsApproved,
                        x.IsLockedOut,
                        x.CreationDate.UtcDateTime,
                        x.LastLoginDate.UtcDateTime,
                        x.LastActivityDate.UtcDateTime,
                        x.LastPasswordChangeDate.UtcDateTime,
                        x.LastLockoutDate.UtcDateTime
                    ));

            #endregion

            #region TypedEntity -> Profile

            this.CreateMap<TypedEntity, Profile>()
                .CreateUsing(x => new Profile());

            #endregion

            #region TypedEntity -> User

            this.CreateMap<TypedEntity, User>()
                .CreateUsing(x => new User());

            #endregion

            #region TypedEntity -> Member

            this.CreateMap<TypedEntity, Member>()
                .CreateUsing(x => new Member());

            #endregion

            #region UserProfile -> User

            this.CreateMap<UserProfile, User>()
                .CreateUsing(x => new User())
                .ForMember(x => x.Id, x => x.Ignore())
                .ForMember(x => x.ProviderUserKey, x => x.Ignore()) // This should always be mapped from MembershipUser
                .ForMember(x => x.ProfileId, x => x.MapFrom(y => y.Id))
                .AfterMap((src, dest) =>
                {
                    dest.SetProviderUserKeyType(src.ProviderUserKeyType);
                });

            #endregion

            #region MemberProfile -> Member

            this.CreateMap<MemberProfile, Member>()
                .CreateUsing(x => new Member())
                .ForMember(x => x.Id, x => x.Ignore())
                .ForMember(x => x.ProviderUserKey, x => x.Ignore()) // This should always be mapped from MembershipUser
                .ForMember(x => x.ProfileId, x => x.MapFrom(y => y.Id))
                .AfterMap((src, dest) =>
                {
                    dest.SetProviderUserKeyType(src.ProviderUserKeyType);
                });

            #endregion

            #region User -> UserProfile

            this.CreateMap<User, UserProfile>()
                .CreateUsing(x => new UserProfile())
                .ForMember(x => x.Id, x => x.MapFrom(y => y.ProfileId))
                .ForMember(x => x.RelationProxies, x => x.Ignore())
                .ForMember(x => x.Attributes, x => x.Ignore())
                .AfterMap((src, dest) =>
                {
                    foreach (var srcAttr in src.Attributes)
                    {
                        var destAttr = dest.Attributes.SingleOrDefault(x => x.AttributeDefinition.Alias == srcAttr.AttributeDefinition.Alias);
                        if (destAttr != null)
                        {
                            foreach (var srcVal in srcAttr.Values)
                            {
                                if (destAttr.Values.ContainsKey(srcVal.Key))
                                    destAttr.Values[srcVal.Key] = srcVal.Value;
                                else
                                    destAttr.Values.Add(srcVal.Key, srcVal.Value);
                            }
                        }
                    }
                });

            #endregion

            #region Member -> MemberProfile

            this.CreateMap<Member, MemberProfile>()
                .CreateUsing(x => new MemberProfile())
                .ForMember(x => x.Id, x => x.MapFrom(y => y.ProfileId))
                .ForMember(x => x.RelationProxies, x => x.Ignore())
                .ForMember(x => x.Attributes, x => x.Ignore())
                .AfterMap((src, dest) =>
                {
                    foreach (var srcAttr in src.Attributes)
                    {
                        var destAttr = dest.Attributes.SingleOrDefault(x => x.AttributeDefinition.Alias == srcAttr.AttributeDefinition.Alias);
                        if (destAttr != null)
                        {
                            foreach (var srcVal in srcAttr.Values)
                            {
                                if (destAttr.Values.ContainsKey(srcVal.Key))
                                    destAttr.Values[srcVal.Key] = srcVal.Value;
                                else
                                    destAttr.Values.Add(srcVal.Key, srcVal.Value);
                            }
                        }
                    }
                });

            #endregion

            #region IRelationById -> PublicAccessInfo

            this.CreateMap<RelationById, PublicAccessInfo>()
                .CreateUsing(x => new PublicAccessInfo())
                .ForMember(x => x.EntityId, x => x.MapFrom(y => y.SourceId))
                .ForMember(x => x.UserGroupIds, x => x.Ignore())
                .ForMember(x => x.LoginPageId, x => x.Ignore())
                .ForMember(x => x.ErrorPageId, x => x.Ignore())
                .AfterMap((src, dest) =>
                {
                    if (src.MetaData != null)
                    {
                        if (src.MetaData.Any(x => x.Key == "UserGroupIds"))
                            dest.UserGroupIds = src.MetaData.SingleOrDefault(x => x.Key == "UserGroupIds").Value.DeserializeJson<IEnumerable<HiveId>>();

                        if (src.MetaData.Any(x => x.Key == "LoginPageId"))
                            dest.LoginPageId = HiveId.Parse(src.MetaData.SingleOrDefault(x => x.Key == "LoginPageId").Value);

                        if (src.MetaData.Any(x => x.Key == "ErrorPageId"))
                            dest.ErrorPageId = HiveId.Parse(src.MetaData.SingleOrDefault(x => x.Key == "ErrorPageId").Value);
                    }
                });

            #endregion
        }
    }
}
