using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using DataAnnotationsExtensions;
using Rebel.Cms.Web.Model.BackOffice.UIElements;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Rebel.Framework.Persistence.Model.Constants.Entities;
using Rebel.Framework.Persistence.Model.Constants.Schemas;
using Rebel.Framework.Security;
using Rebel.Hive.ProviderGrouping;

namespace Rebel.Cms.Web.Model.BackOffice.Editors
{
    public class MembershipUserEditorModel : ProfileEditorModel, IValidatableObject
    {
        public MembershipUserEditorModel()
        {
            Groups = new List<HiveId>();
        }

        [Required]
        public string Username { get; set; }

        [DataType(global::System.ComponentModel.DataAnnotations.DataType.Password)]
        public string Password { get; set; }

        [EqualTo("Password")]
        [DataType(global::System.ComponentModel.DataAnnotations.DataType.Password)]
        public string ConfirmPassword { get; set; }

        [DataType(global::System.ComponentModel.DataAnnotations.DataType.Password)]
        public string OldPassword { get; set; }

        [Required]
        [Email]
        [DataType(global::System.ComponentModel.DataAnnotations.DataType.EmailAddress)]
        public string Email { get; set; }

        public bool IsApproved { get; set; }

        [ReadOnly(true)]
        public DateTime? LastLoginDate { get; set; }

        [ReadOnly(true)]
        public DateTime? LastActivityDate { get; set; }

        [ReadOnly(true)]
        public DateTime? LastPasswordChangeDate { get; set; }

        [Required]
        public IEnumerable<HiveId> Groups { get; set; }

        /// <summary>
        /// Validates the input
        /// </summary>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // To be overwritten
            return Enumerable.Empty<ValidationResult>();
        }

        protected override void PopulateUIElements()
        {
            UIElements.Clear();
            UIElements.Add(new SaveButtonUIElement());
        }
    }
}