using System.ComponentModel.DataAnnotations;
using Rebel.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Rebel.Tests.Extensions.Stubs.PropertyEditors
{
    public class TestEditorModel : EditorModel
    {
        [Required(AllowEmptyStrings = false)]
        public string Value { get; set; }

    }
}