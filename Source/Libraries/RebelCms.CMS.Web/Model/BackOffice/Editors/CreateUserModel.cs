using System.ComponentModel.DataAnnotations;

namespace RebelCms.Cms.Web.Model.BackOffice.Editors
{
    /// <summary>
    /// Model representing the create new wizard view for a user
    /// </summary>
    public class CreateUserModel
    {
        [Required]
        public string Username { get; set; }

    }
}
