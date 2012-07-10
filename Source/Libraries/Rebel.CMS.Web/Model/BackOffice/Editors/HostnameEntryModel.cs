using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Rebel.Framework;

namespace Rebel.Cms.Web.Model.BackOffice.Editors
{
    /// <summary>
    /// A view model representing one hostname
    /// </summary>
    public class HostnameEntryModel
    {
        /// <summary>
        /// The ID of the hostname entity
        /// </summary>
        [HiddenInput(DisplayValue = false)]
        public HiveId Id { get; set; }

        [Required]
        public string Hostname { get; set; }

        public int SortOrder { get; set; }
    }
}