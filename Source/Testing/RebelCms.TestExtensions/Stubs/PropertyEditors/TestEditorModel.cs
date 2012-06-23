using System.ComponentModel.DataAnnotations;
using RebelCms.Cms.Web.Model.BackOffice.PropertyEditors;

namespace RebelCms.Tests.Extensions.Stubs.PropertyEditors
{
    public class TestEditorModel : EditorModel
    {
        [Required(AllowEmptyStrings = false)]
        public string Value { get; set; }

    }
}