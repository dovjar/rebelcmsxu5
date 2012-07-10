using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using DataAnnotationsExtensions;
using Rebel.Framework.Persistence.Model.Constants;

namespace Rebel.Cms.Web.Model.BackOffice.Editors
{
    /// <summary>
    /// Model representing the create new wizard view for a user
    /// </summary>
    [Bind(Exclude = "NoticeBoard")]
    public class CreateUserModel : CreateContentModel
    {
        public CreateUserModel()
        {
            AvailableProfileTypes = new List<SelectListItem>();
        }

        [Required]
        public string Username { get; set; }

        [Required]
        [Email]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public IEnumerable<SelectListItem> AvailableProfileTypes { get; set; }
    }
}
