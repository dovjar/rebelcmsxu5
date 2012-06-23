using System.Web.Mvc;
using RebelCms.Cms.Web.Mvc.Validation;
using RebelCms.Framework;

namespace RebelCms.Cms.Web.Model.BackOffice.Editors
{
    /// <summary>
    /// Model representing the content sort editor
    /// </summary>
    public class SortModel : DialogModel
    {
        [HiddenInput(DisplayValue = false)]
        [HiveIdRequired]
        public HiveId ParentId { get; set; }


        public SortItem[] Items { get; set; }
    }
}