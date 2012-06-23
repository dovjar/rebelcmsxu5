using System.ComponentModel;

namespace RebelCms.Cms.Web.Model.BackOffice.Editors
{
    /// <summary>
    /// Used to render the create document type dialog
    /// </summary>
    public class CreateDocumentTypeModel : CreateContentModel
    {
        public CreateDocumentTypeModel()
        {
            CreateTemplate = true;
        }

        [DisplayName("Create matching template?")]
        public bool CreateTemplate { get; set; }
    }
}