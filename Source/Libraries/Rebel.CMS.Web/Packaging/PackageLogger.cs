using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using NuGet;
using Rebel.Cms.Web.Context;
using Rebel.Framework;

namespace Rebel.Cms.Web.Packaging
{
    public class PackageLogger
    {
        private readonly IBackOfficeRequestContext _context;
        private readonly IPackage _package;
        private readonly HttpContextBase _httpContext;
        private List<PackageLogEntry> _entries;
        private string _absoluteLogFilePath;

        public PackageLogger(IBackOfficeRequestContext context, HttpContextBase httpContext, IPackage package,
            bool autoHydrate = false)
        {
            _context = context;
            _httpContext = httpContext;
            _package = package;

            var packageFolderName = _context.PackageContext.LocalPathResolver.GetPackageDirectory(_package);
            var packageFolderPath = Path.Combine(_context.Application.Settings.PluginConfig.PluginsPath, "Packages", packageFolderName);
            var logFilePath = Path.Combine(packageFolderPath, "Log.json");
            _absoluteLogFilePath = _httpContext.Server.MapPath(logFilePath);

            _entries = new List<PackageLogEntry>();

            if(autoHydrate)
                Hydrate();
        }

        public void Hydrate()
        {
            if (File.Exists(_absoluteLogFilePath))
            {
                var contents = File.ReadAllText(_absoluteLogFilePath);
                _entries.AddRange(contents.DeserializeJson<IEnumerable<PackageLogEntry>>());
            }
        }

        public void Log(bool success, string message)
        {
            Log(new PackageLogEntry{ Success = success, Message = message });
        }

        public void Log(PackageLogEntry entry)
        {
            _entries.Add(entry);
        }

        public void Persist()
        {
            var content = _entries.ToJsonString();
            var sw = new StreamWriter(_absoluteLogFilePath, false);
            sw.Write(content);
            sw.Close();
        }

        public IEnumerable<PackageLogEntry> GetLogEntries()
        {
            return _entries;
        }
    }
}
