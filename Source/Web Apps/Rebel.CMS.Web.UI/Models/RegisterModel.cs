using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataAnnotationsExtensions;
using Rebel.Framework;

namespace Rebel.Cms.Web.UI.Models
{
    public class RegisterModel : IValidatableObject
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [Email]
        public string Email { get; set; }

        [Required]
        [UIHint("Password")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Confirm Password")]
        [UIHint("Password")]
        public string ConfirmPassword { get; set; }

        public bool RequiresQuestionAndAnswer { get; set; }

        public string SecurityQuestion { get; set; }

        public string SecurityAnswer { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string MemberTypeAlias { get; set; }

        [HiddenInput(DisplayValue = false)]
        public HiveId MemberGroupId { get; set; }

        [HiddenInput(DisplayValue = false)]
        public HiveId SuccessRedirectId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if(ConfirmPassword != Password)
            {
                yield return new ValidationResult("Passwords do not match.", new[] { "ConfirmPassword" });
            }

            if(RequiresQuestionAndAnswer)
            {
                if(string.IsNullOrWhiteSpace(SecurityQuestion))
                {
                    yield return new ValidationResult("The Security Question field is required.", new[] { "SecurityQuestion" });
                }
                if (string.IsNullOrWhiteSpace(SecurityAnswer))
                {
                    yield return new ValidationResult("The Security Answer field is required.", new[] { "SecurityAnswer" });
                }
            }
        }
    }
}