using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Rebel.Framework.Persistence.Model
{
    public class PackageDefinition
    {
        [JsonIgnore]
        public HiveId Id { get; set; }

        public string Name { get; set; }
        public string Alias { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }

        public string ProjectUrl { get; set; }
        public string LicenseUrl { get; set; }

        public string Description { get; set; }
        public string Tags { get; set; }

        public HiveId ContentNodeId { get; set; }
        public bool IncludeChildContentNodes { get; set; }

        public HiveId MediaNodeId { get; set; }
        public bool IncludeChildMediaNodes { get; set; }

        public IEnumerable<HiveId> DocumentTypeIds { get; set; }
        public IEnumerable<HiveId> MediaTypeIds { get; set; }
        public IEnumerable<HiveId> TemplateIds { get; set; }
        public IEnumerable<HiveId> PartialIds { get; set; }
        public IEnumerable<HiveId> StylesheetIds { get; set; }
        public IEnumerable<HiveId> ScriptIds { get; set; }
        public IEnumerable<HiveId> MacroIds { get; set; }
        public IEnumerable<string> LanguageIds { get; set; }
        public IEnumerable<HiveId> DictionaryItemIds { get; set; }
        public IEnumerable<HiveId> DataTypeIds { get; set; }

        public IEnumerable<string> AdditionalFiles { get; set; }
        public string Config { get; set; }
    }
}
