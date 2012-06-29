using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using System.Xml.Linq;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Abstractions.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Attribution;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.AttributeTypes;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;
using Umbraco.Framework.Persistence.Model.Constants.SerializationTypes;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Hive.Providers.Membership.Mapping
{
    /// <summary>
    /// Mapping class to map Hive entities to Examine entities and vice versa
    /// </summary>
    public sealed class MembershipWrapperModelMapper : AbstractFluentMappingEngine
    {
        private readonly IAttributeTypeRegistry _attributeTypeRegistry;

        public MembershipWrapperModelMapper(IAttributeTypeRegistry attributeTypeRegistry, IFrameworkContext frameworkContext)
            : base(frameworkContext)
        {
            _attributeTypeRegistry = attributeTypeRegistry;
        }


        public override void ConfigureMappings()
        {

            #region MembershipUser -> TypedEntity

            this.CreateMap<MembershipUser, TypedEntity>(true)
                .CreateUsing(x => new TypedEntity())
                .ForMember(x => x.Id, opt => opt.MapFrom(y => HiveId.Parse(y.ProviderUserKey.ToString())))
                .MapMemberFrom(x => x.UtcCreated, x => x.CreationDate)
                .AfterMap((x, t) =>
                    {
                        var textType = _attributeTypeRegistry.GetAttributeType(StringAttributeType.AliasValue);
                        var readOnlyType = _attributeTypeRegistry.GetAttributeType(ReadOnlyAttributeType.AliasValue);
                        var boolType = _attributeTypeRegistry.GetAttributeType(BoolAttributeType.AliasValue);
                        var group = FixedGroupDefinitions.MemberDetails;

                        //Need to line up the attribute name fields to match that of Member

                        //TODO: We need to store our own version of the name as seperate data than what the underlying membership provider can store
                        t.Attributes.SetValueOrAdd(new TypedAttribute(new AttributeDefinition(NodeNameAttributeDefinition.AliasValue, "Name")
                        {
                            AttributeGroup = group,
                            AttributeType = textType
                        }, x.UserName));

                        t.Attributes.SetValueOrAdd(new TypedAttribute(new AttributeDefinition(MembershipUserSchema.UsernameAlias, "Username")
                            {
                                AttributeGroup = group,
                                AttributeType = textType
                            }, x.UserName));
                        t.Attributes.SetValueOrAdd(new TypedAttribute(new AttributeDefinition(MembershipUserSchema.EmailAlias, "Email")
                        {
                            AttributeGroup = group,
                            AttributeType = textType
                        }, x.Email));
                        t.Attributes.SetValueOrAdd(new TypedAttribute(new AttributeDefinition(MembershipUserSchema.PasswordQuestionAlias, "Password Question")
                        {
                            AttributeGroup = group,
                            AttributeType = textType
                        }, x.PasswordQuestion));
                        t.Attributes.SetValueOrAdd(new TypedAttribute(new AttributeDefinition(MembershipUserSchema.CommentsAlias, "Comment")
                        {
                            AttributeGroup = group,
                            AttributeType = textType
                        }, x.Comment));
                        t.Attributes.SetValueOrAdd(new TypedAttribute(new AttributeDefinition(MembershipUserSchema.IsApprovedAlias, "Is Approved")
                        {
                            AttributeGroup = group,
                            AttributeType = boolType
                        }, x.IsApproved));
                        t.Attributes.SetValueOrAdd(new TypedAttribute(new AttributeDefinition(MembershipUserSchema.IsLockedOutAlias, "Is Locked Out")
                        {
                            AttributeGroup = group,
                            AttributeType = boolType
                        }, x.IsLockedOut));
                        t.Attributes.SetValueOrAdd(new TypedAttribute(new AttributeDefinition(MembershipUserSchema.LastLockoutDateAlias, "Last Lockout Date")
                        {
                            AttributeGroup = group,
                            AttributeType = readOnlyType
                        }, new DateTimeOffset(x.LastLockoutDate)));
                        t.Attributes.SetValueOrAdd(new TypedAttribute(new AttributeDefinition(MembershipUserSchema.LastLoginDateAlias, "Last Login Date")
                        {
                            AttributeGroup = group,
                            AttributeType = readOnlyType
                        }, new DateTimeOffset(x.LastLoginDate)));
                        t.Attributes.SetValueOrAdd(new TypedAttribute(new AttributeDefinition(MembershipUserSchema.LastActivityDateAlias, "Last Activity Date")
                        {
                            AttributeGroup = group,
                            AttributeType = readOnlyType
                        }, new DateTimeOffset(x.LastActivityDate)));
                        t.Attributes.SetValueOrAdd(new TypedAttribute(new AttributeDefinition(MembershipUserSchema.LastPasswordChangeDateAlias, "Last Password Changed Date")
                        {
                            AttributeGroup = group,
                            AttributeType = readOnlyType
                        }, new DateTimeOffset(x.LastPasswordChangedDate)));
                        t.Attributes.SetValueOrAdd(new TypedAttribute(new AttributeDefinition(MembershipUserSchema.IsOnlineAlias, "Is Online")
                        {
                            AttributeGroup = group,
                            AttributeType = readOnlyType
                        }, x.IsOnline));
                    });

            #endregion

            #region MembershipUser -> Member

            this.CreateMap<MembershipUser, Member>(true)
                .CreateUsing(x => new Member())
                //TODO: We need to store our own version of the name as seperate data than what the underlying membership provider can store
                .MapMemberFrom(x => x.Name, x => x.UserName)
                .MapMemberFrom(x => x.Username, x => x.UserName)
                .MapMemberFrom(x => x.Comments, x => x.Comment)
                .MapMemberFrom(x => x.UtcCreated, x => x.CreationDate)
                .MapMemberFrom(x => x.LastPasswordChangeDate, x => x.LastPasswordChangedDate)
                .ForMember(x => x.Id, opt => opt.MapFrom(y => HiveId.Parse(y.ProviderUserKey.ToString())));

            #endregion

            #region TypedEntity -> MembershipUser

            this.CreateMap<TypedEntity, MembershipUser>(true)
                //NOTE: This will NOT work for new entities, only existing ones since we'll never know the provider name
                .CreateUsing(x => new MembershipUser("",
                        x.Attribute<string>(NodeNameAttributeDefinition.AliasValue),
                        x.Id.Value.Value,
                        x.Attribute<string>(MembershipUserSchema.EmailAlias),
                        x.Attribute<string>(MembershipUserSchema.PasswordQuestionAlias),
                        x.Attribute<string>(MembershipUserSchema.CommentsAlias),
                        x.Attribute<bool>(MembershipUserSchema.IsApprovedAlias),
                        x.Attribute<bool>(MembershipUserSchema.IsLockedOutAlias),
                        x.UtcCreated.UtcDateTime,
                        x.Attribute<DateTimeOffset>(MembershipUserSchema.LastLoginDateAlias).UtcDateTime,
                        x.Attribute<DateTimeOffset>(MembershipUserSchema.LastActivityDateAlias).UtcDateTime,
                        x.Attribute<DateTimeOffset>(MembershipUserSchema.LastPasswordChangeDateAlias).UtcDateTime,
                        x.Attribute<DateTimeOffset>(MembershipUserSchema.LastLockoutDateAlias).UtcDateTime
                    ))
                //these are the only writable properties for MembershipUser
                .MapMemberFrom(x => x.Email, x => x.Attribute<string>(MembershipUserSchema.EmailAlias))
                .MapMemberFrom(x => x.Comment, x => x.Attribute<string>(MembershipUserSchema.CommentsAlias))
                .MapMemberFrom(x => x.IsApproved, x => x.Attribute<bool>(MembershipUserSchema.IsApprovedAlias))
                .MapMemberFrom(x => x.LastLoginDate, x => x.Attribute<DateTimeOffset>(MembershipUserSchema.LastLoginDateAlias).UtcDateTime)
                .MapMemberFrom(x => x.LastActivityDate, x => x.Attribute<DateTimeOffset>(MembershipUserSchema.LastActivityDateAlias).UtcDateTime);

            #endregion

        }

    }
}
