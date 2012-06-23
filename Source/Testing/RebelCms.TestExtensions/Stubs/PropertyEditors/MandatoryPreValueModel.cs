using System.ComponentModel.DataAnnotations;
using RebelCms.Cms.Web.Model.BackOffice.PropertyEditors;

namespace RebelCms.Tests.Extensions.Stubs.PropertyEditors
{


    public class MandatoryPreValueModel : PreValueModel
    {
        [Required]
        public string Value { get; set; }

    }
}
