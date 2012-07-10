using System.ComponentModel.DataAnnotations;
using Rebel.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Rebel.Tests.Extensions.Stubs.PropertyEditors
{


    public class MandatoryPreValueModel : PreValueModel
    {
        [Required]
        public string Value { get; set; }

    }
}
