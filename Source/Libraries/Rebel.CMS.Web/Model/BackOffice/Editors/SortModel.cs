using System.Web.Mvc;
using Rebel.Cms.Web.Mvc.Validation;
using Rebel.Framework;

namespace Rebel.Cms.Web.Model.BackOffice.Editors
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