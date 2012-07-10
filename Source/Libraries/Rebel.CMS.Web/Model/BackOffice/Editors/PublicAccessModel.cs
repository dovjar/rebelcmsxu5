using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Newtonsoft.Json;
using Rebel.Framework;

namespace Rebel.Cms.Web.Model.BackOffice.Editors
{
    public class PublicAccessModel : DialogModel
    {
        public HiveId Id { get; set; }

        [Required]
        public IEnumerable<HiveId> UserGroupIds { get; set; }

        [Required]
        public HiveId LoginPageId { get; set; }

        [Required]
        public HiveId ErrorPageId { get; set; }

        public IEnumerable<SelectListItem> AvailableUserGroups { get; set; }

        public PublicAccessModel()
        {
            UserGroupIds = new List<HiveId>();
            AvailableUserGroups = new List<SelectListItem>();
        }
    }
}
