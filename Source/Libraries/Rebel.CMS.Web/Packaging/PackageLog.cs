using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rebel.Cms.Web.Packaging
{
    public class PackageLog
    {
        public string PackageId { get; set; }
        public string Title { get; set; }
        public Version Version { get; set; }
        public Uri ProjectUrl { get; set; }
        public IEnumerable<PackageLogEntry> LogEntries { get; set; }
    }
}
