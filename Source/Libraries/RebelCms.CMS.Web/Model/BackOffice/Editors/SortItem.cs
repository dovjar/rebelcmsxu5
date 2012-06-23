using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using RebelCms.Cms.Web.Mvc.Validation;
using RebelCms.Framework;

namespace RebelCms.Cms.Web.Model.BackOffice.Editors
{
    /// <summary>
    /// Model representing an item in the sort editor
    /// </summary>
    [Bind(Exclude = "UtcCreated,Name")]
    public class SortItem
    {
        [HiddenInput(DisplayValue = false)]
        [HiveIdRequired]
        public HiveId Id { get; set; }
        
        [ReadOnly(true)]
        public DateTimeOffset UtcCreated { get; set; }

        [Required]
        public int SortIndex { get; set; }
        
        [ReadOnly(true)]
        public string Name { get; set; }
    }
}