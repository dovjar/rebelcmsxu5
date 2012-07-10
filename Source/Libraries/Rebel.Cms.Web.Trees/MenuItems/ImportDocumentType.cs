using ClientDependency.Core;
using Rebel.Cms.Web.Model.BackOffice.Trees;

namespace Rebel.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("A98F9C68-9A68-426E-AFCC-AD21F3511241",
        "Import document type",
        "Rebel.Controls.MenuItems.ImportDocumentType",
        "menu-import-doc-type")]
    public class ImportDocumentType : MenuItem { }
}