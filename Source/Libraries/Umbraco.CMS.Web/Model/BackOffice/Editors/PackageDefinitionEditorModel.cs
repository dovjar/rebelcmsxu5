using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Cms.Web.Model.BackOffice.UIElements;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    public class PackageDefinitionEditorModel : EditorModel
    {
        [Required]
        [RegularExpression("^[0-9a-zA-Z]*$", ErrorMessage = "Aliases can only contain alphanumeric characters")]
        public string Alias { get; set; }

        [Required]
        [RegularExpression("^[0-9]+.[0-9]+(.[0-9]+(.[0-9]+)?)?$", ErrorMessage = "Version numbers must contain only numbers and periods, and must consist of at least a Major and Minor version number (ie 1.0)")]
        public string Version { get; set; }
        [Required]
        public string Author { get; set; }

        [Required]
        [Display(Name = "Project Url")]
        [RegularExpression("^http://.*", ErrorMessage = "Not a valid URL")]
        public string ProjectUrl { get; set; }
        [Display(Name = "License Url")]
        [RegularExpression("^http://.*", ErrorMessage = "Not a valid URL")]
        public string LicenseUrl { get; set; }

        [DataType(global::System.ComponentModel.DataAnnotations.DataType.MultilineText)]
        public string Description { get; set; }

        public string Tags { get; set; }

        public HiveId ContentNodeId { get; set; }
        [Display(Name = "Include child nodes?")]
        public bool IncludeChildContentNodes { get; set; }

        public HiveId MediaNodeId { get; set; }
        [Display(Name = "Include child nodes?")]
        public bool IncludeChildMediaNodes { get; set; }

        [Display(Name = "Document Types")]
        public IEnumerable<HiveId> DocumentTypeIds { get; set; }
        [Display(Name = "Media Types")]
        public IEnumerable<HiveId> MediaTypeIds { get; set; }
        [Display(Name = "Templates")]
        public IEnumerable<HiveId> TemplateIds { get; set; }
        [Display(Name = "Partials")]
        public IEnumerable<HiveId> PartialIds { get; set; }
        [Display(Name = "Stylesheets")]
        public IEnumerable<HiveId> StylesheetIds { get; set; }
        [Display(Name = "Scripts")]
        public IEnumerable<HiveId> ScriptIds { get; set; }
        [Display(Name = "Macros")]
        public IEnumerable<HiveId> MacroIds { get; set; }
        [Display(Name = "Languages")]
        public IEnumerable<string> LanguageIds { get; set; }
        [Display(Name = "Dictionary Items")]
        public IEnumerable<HiveId> DictionaryItemIds { get; set; }
        [Display(Name = "Data Types")]
        public IEnumerable<HiveId> DataTypeIds { get; set; }

        [Display(Name = "Additional Files")]
        public IEnumerable<string> AdditionalFiles { get; set; }

        public string Config { get; set; }

        public bool IsPublished { get; set; }

        public IEnumerable<SelectListItem> AvailableDocumentTypes { get; set; }
        public IEnumerable<SelectListItem> AvailableMediaTypes { get; set; }
        public IEnumerable<SelectListItem> AvailableTemplates { get; set; }
        public IEnumerable<SelectListItem> AvailablePartials { get; set; }
        public IEnumerable<SelectListItem> AvailableStylesheets { get; set; }
        public IEnumerable<SelectListItem> AvailableScripts { get; set; }
        public IEnumerable<SelectListItem> AvailableMacros { get; set; }
        public IEnumerable<SelectListItem> AvailableLanguages { get; set; }
        public IEnumerable<SelectListItem> AvailableDictionaryItems { get; set; }
        public IEnumerable<SelectListItem> AvailableDataTypes { get; set; }

        public int ActiveTabIndex { get; set; }

        public PackageDefinitionEditorModel()
        {
            DocumentTypeIds = Enumerable.Empty<HiveId>();
            MediaTypeIds = Enumerable.Empty<HiveId>();
            TemplateIds = Enumerable.Empty<HiveId>();
            PartialIds = Enumerable.Empty<HiveId>();
            StylesheetIds = Enumerable.Empty<HiveId>();
            ScriptIds = Enumerable.Empty<HiveId>();
            MacroIds = Enumerable.Empty<HiveId>();
            LanguageIds = Enumerable.Empty<string>();
            DictionaryItemIds = Enumerable.Empty<HiveId>();
            DataTypeIds = Enumerable.Empty<HiveId>();
            AdditionalFiles = Enumerable.Empty<string>();

            Config = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<configuration>

  <system.web>
    <pages>
      <namespaces>
        <add namespace=""System.Web.Mvc"" />
      </namespaces>
    </pages>
  </system.web>

</configuration>";

            PopulateUIElements();
        }

        protected virtual void PopulateUIElements()
        {
            UIElements.Clear();
            UIElements.Add(new SaveButtonUIElement());
            UIElements.Add(new PublishButtonUIElement());
        }
    }
}
