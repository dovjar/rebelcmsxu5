using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.AttributeTypes;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;

namespace Umbraco.Framework.Security.Model.Schemas
{
    public class MembershipUserSchema : SystemSchema
    {
        public const string SchemaAlias = "system-membership-user";
        public const string IsLockedOutAlias = "isLockedOut";
        public const string IsOnlineAlias = "isOnline";
        public const string LastLockoutDateAlias = "lastLockedOutDate";
        public const string UsernameAlias = "username";
        public const string PasswordAlias = "password";
        public const string PasswordSaltAlias = "passwordSalt";
        public const string PasswordQuestionAlias = "passwordQuestion";
        public const string PasswordAnswerAlias = "passwordAnswer";
        public const string CommentsAlias = "comments";
        public const string EmailAlias = "email";
        public const string IsApprovedAlias = "isApproved";
        public const string LastLoginDateAlias = "lastLoginDate";
        public const string LastActivityDateAlias = "lastActivityDate";
        public const string LastPasswordChangeDateAlias = "lastPasswordChangeDate";

        public MembershipUserSchema()
        {
            this.Setup(SchemaAlias, "Membership User");

            Id = FixedHiveIds.MembershipUserSchema;
            SchemaType = FixedSchemaTypes.MembershipUser;

            CreatedAttributeDefinitions();
        }

        protected virtual AttributeGroup DetailsGroup
        {
            get { return FixedGroupDefinitions.UserDetails; }
        }

        protected virtual void CreatedAttributeDefinitions()
        {
            var inBuiltTextType = AttributeTypeRegistry.Current.GetAttributeType(StringAttributeType.AliasValue);
            var inBuiltBoolType = AttributeTypeRegistry.Current.GetAttributeType(BoolAttributeType.AliasValue);
            var inBuiltReadOnlyType = AttributeTypeRegistry.Current.GetAttributeType(ReadOnlyAttributeType.AliasValue);
            var inBuiltDateTimeAttributeType = AttributeTypeRegistry.Current.GetAttributeType(DateTimeAttributeType.AliasValue);

            this.AttributeDefinitions.Add(new AttributeDefinition(NodeNameAttributeDefinition.AliasValue, "Node Name")
                {
                    AttributeType = inBuiltTextType,
                    AttributeGroup = DetailsGroup,
                    Ordinal = 0
                });

            this.AttributeDefinitions.Add(new AttributeDefinition(IsOnlineAlias, "Is Online")
                {
                    AttributeType = inBuiltReadOnlyType,
                    AttributeGroup = DetailsGroup,
                    Ordinal = 1
                });

            this.AttributeDefinitions.Add(new AttributeDefinition(IsLockedOutAlias, "Is Locked Out")
                {
                    AttributeType = inBuiltBoolType,
                    AttributeGroup = DetailsGroup,
                    Ordinal = 1
                });

            this.AttributeDefinitions.Add(new AttributeDefinition(LastLockoutDateAlias, "Last Lockout Date")
                {
                    AttributeType = inBuiltReadOnlyType,
                    AttributeGroup = DetailsGroup,
                    Ordinal = 1
                });

            this.AttributeDefinitions.Add(new AttributeDefinition(UsernameAlias, "Username")
                {
                    AttributeType = inBuiltTextType,
                    AttributeGroup = DetailsGroup,
                    Ordinal = 1
                });

            this.AttributeDefinitions.Add(new AttributeDefinition(PasswordAlias, "Password")
                {
                    AttributeType = inBuiltTextType,
                    AttributeGroup = DetailsGroup,
                    Ordinal = 2
                });

            this.AttributeDefinitions.Add(new AttributeDefinition(PasswordSaltAlias, "Salt")
            {
                AttributeType = inBuiltReadOnlyType,
                AttributeGroup = DetailsGroup,
                Ordinal = 3
            });

            this.AttributeDefinitions.Add(new AttributeDefinition(PasswordQuestionAlias, "Password question")
                {
                    AttributeType = inBuiltTextType,
                    AttributeGroup = DetailsGroup,
                    Ordinal = 4
                });

            this.AttributeDefinitions.Add(new AttributeDefinition(PasswordAnswerAlias, "Password answer")
                {
                    AttributeType = inBuiltTextType,
                    AttributeGroup = DetailsGroup,
                    Ordinal = 5
                });

            this.AttributeDefinitions.Add(new AttributeDefinition(EmailAlias, "Email address")
                {
                    AttributeType = inBuiltTextType,
                    AttributeGroup = DetailsGroup,
                    Ordinal = 6
                });

            this.AttributeDefinitions.Add(new AttributeDefinition(CommentsAlias, "Comments / Notes")
                {
                    AttributeType = inBuiltTextType,
                    AttributeGroup = DetailsGroup,
                    Ordinal = 7
                });

            this.AttributeDefinitions.Add(new AttributeDefinition(IsApprovedAlias, "Is user approved?")
                {
                    AttributeType = inBuiltBoolType,
                    AttributeGroup = DetailsGroup,
                    Ordinal = 8
                });

            this.AttributeDefinitions.Add(new AttributeDefinition(LastLoginDateAlias, "Last login date")
                {
                    AttributeType = inBuiltDateTimeAttributeType,
                    AttributeGroup = DetailsGroup,
                    Ordinal = 10
                });

            this.AttributeDefinitions.Add(new AttributeDefinition(LastActivityDateAlias, "Last activity date")
                {
                    AttributeType = inBuiltDateTimeAttributeType,
                    AttributeGroup = DetailsGroup,
                    Ordinal = 11
                });

            this.AttributeDefinitions.Add(new AttributeDefinition(LastPasswordChangeDateAlias, "Last password change date")
                {
                    AttributeType = inBuiltDateTimeAttributeType,
                    AttributeGroup = DetailsGroup,
                    Ordinal = 12
                });
        }

    }
}