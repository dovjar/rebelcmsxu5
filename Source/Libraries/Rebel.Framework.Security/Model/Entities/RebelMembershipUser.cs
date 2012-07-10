using System;
using Rebel.Framework.Persistence;
using Rebel.Framework.Persistence.Model.Constants.Schemas;
using Rebel.Framework.Persistence.ModelFirst;
using Rebel.Framework.Persistence.ModelFirst.Annotations;
using Rebel.Framework.Security.Model.Schemas;

namespace Rebel.Framework.Security.Model.Entities
{
    using Rebel.Framework.Linq;
    using Rebel.Framework.Linq.CriteriaGeneration.StructureMetadata;
    using Rebel.Framework.Persistence.Model.LinqSupport;

    [QueryStructureBinderOfType(typeof(AnnotationQueryStructureBinder))]
    [DefaultSchemaForQuerying(SchemaAlias = MembershipUserSchema.SchemaAlias)]
    public class RebelMembershipUser : CustomTypedEntity<RebelMembershipUser>
    {
        public RebelMembershipUser()
        {
            this.SetupFromSchema<MembershipUserSchema>();
        }

        /// <summary>
        /// The unique identifier for the user for use with Membership services
        /// </summary>
        public object ProviderUserKey
        {
            get { return Id.Value.Value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is online.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is online; otherwise, <c>false</c>.
        /// </value>
        [AttributeAlias(Alias = MembershipUserSchema.IsOnlineAlias)]
        public bool IsOnline
        {
            get { return base.BaseAutoGet(x => x.IsOnline); }
            set { base.BaseAutoSet(x => x.IsOnline, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is locked out.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is locked out; otherwise, <c>false</c>.
        /// </value>
        [AttributeAlias(Alias = MembershipUserSchema.IsLockedOutAlias)]
        public bool IsLockedOut
        {
            get { return base.BaseAutoGet(x => x.IsLockedOut); }
            set { base.BaseAutoSet(x => x.IsLockedOut, value); }
        }

        /// <summary>
        /// Gets or sets the last lockout date.
        /// </summary>
        /// <value>
        /// The last lockout date.
        /// </value>
        [AttributeAlias(Alias = MembershipUserSchema.LastLockoutDateAlias)]
        public DateTimeOffset LastLockoutDate
        {
            get { return base.BaseAutoGet(x => x.LastLockoutDate); }
            set { base.BaseAutoSet(x => x.LastLockoutDate, value); }
        }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>
        /// The username.
        /// </value>
        [AttributeAlias(Alias = MembershipUserSchema.UsernameAlias)]
        public string Username
        {
            get { return base.BaseAutoGet(x => x.Username); }
            set { base.BaseAutoSet(x => x.Username, value); }
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        [AttributeAlias(Alias = MembershipUserSchema.PasswordAlias)]
        public string Password
        {
            get { return base.BaseAutoGet(x => x.Password); }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    base.BaseAutoSet(x => x.Password, value);
            }
        }
        
        /// <summary>
        /// Gets or sets the password salt.
        /// </summary>
        /// <value>
        /// The password salt.
        /// </value>
        [AttributeAlias(Alias = MembershipUserSchema.PasswordSaltAlias)]
        public string PasswordSalt
        {
            get { return base.BaseAutoGet(x => x.PasswordSalt); }
            set { base.BaseAutoSet(x => x.PasswordSalt, value); }
        }

        /// <summary>
        /// Gets or sets the password question.
        /// </summary>
        /// <value>
        /// The password question.
        /// </value>
        [AttributeAlias(Alias = MembershipUserSchema.PasswordQuestionAlias)]
        public string PasswordQuestion
        {
            get { return base.BaseAutoGet(x => x.PasswordQuestion); }
            set { base.BaseAutoSet(x => x.PasswordQuestion, value); }
        }

        /// <summary>
        /// Gets or sets the password answer.
        /// </summary>
        /// <value>
        /// The password answer.
        /// </value>
        [AttributeAlias(Alias = MembershipUserSchema.PasswordAnswerAlias)]
        public string PasswordAnswer
        {
            get { return base.BaseAutoGet(x => x.PasswordAnswer); }
            set { base.BaseAutoSet(x => x.PasswordAnswer, value); }
        }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        [AttributeAlias(Alias = MembershipUserSchema.EmailAlias)]
        public string Email
        {
            get { return base.BaseAutoGet(x => x.Email); }
            set { base.BaseAutoSet(x => x.Email, value); }
        }

        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        /// <value>
        /// The comments.
        /// </value>
        [AttributeAlias(Alias = MembershipUserSchema.CommentsAlias)]
        public string Comments
        {
            get { return base.BaseAutoGet(x => x.Comments); }
            set { base.BaseAutoSet(x => x.Comments, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is approved.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is approved; otherwise, <c>false</c>.
        /// </value>
        [AttributeAlias(Alias = MembershipUserSchema.IsApprovedAlias)]
        public bool IsApproved
        {
            get { return base.BaseAutoGet(x => x.IsApproved); }
            set { base.BaseAutoSet(x => x.IsApproved, value); }
        }

        /// <summary>
        /// Gets or sets the last login date.
        /// </summary>
        /// <value>
        /// The last login date.
        /// </value>
        [AttributeAlias(Alias = MembershipUserSchema.LastLoginDateAlias)]
        public DateTimeOffset LastLoginDate
        {
            get { return base.BaseAutoGet(x => x.LastLoginDate, DateTimeOffset.MinValue); }
            set { base.BaseAutoSet(x => x.LastLoginDate, value); }
        }

        /// <summary>
        /// Gets or sets the last activity date.
        /// </summary>
        /// <value>
        /// The last activity date.
        /// </value>
        [AttributeAlias(Alias = MembershipUserSchema.LastActivityDateAlias)]
        public DateTimeOffset LastActivityDate
        {
            get { return base.BaseAutoGet(x => x.LastActivityDate, DateTimeOffset.MinValue); }
            set { base.BaseAutoSet(x => x.LastActivityDate, value); }
        }

        /// <summary>
        /// Gets or sets the last password change date.
        /// </summary>
        /// <value>
        /// The last password change date.
        /// </value>
        [AttributeAlias(Alias = MembershipUserSchema.LastPasswordChangeDateAlias)]
        public DateTimeOffset LastPasswordChangeDate
        {
            get { return base.BaseAutoGet(x => x.LastPasswordChangeDate, DateTimeOffset.MinValue); }
            set { base.BaseAutoSet(x => x.LastPasswordChangeDate, value); }
        }
    }
}
