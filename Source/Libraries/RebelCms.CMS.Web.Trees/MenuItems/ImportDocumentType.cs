using ClientDependency.Core;
using RebelCms.Cms.Web.Model.BackOffice.Trees;

namespace RebelCms.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("A98F9C68-9A68-426E-AFCC-AD21F3511241",
        "Import document type",
        "RebelCms.Controls.MenuItems.ImportDocumentType",
        "menu-import-doc-type")]
    public class ImportDocumentType : MenuItem { }
}