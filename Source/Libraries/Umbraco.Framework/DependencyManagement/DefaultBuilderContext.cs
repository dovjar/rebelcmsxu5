
using Umbraco.Framework.Configuration;

namespace Umbraco.Framework.DependencyManagement
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Web;
    using System.Web.Hosting;

    public class DefaultBuilderContext : IBuilderContext
    {
        private readonly IConfigurationResolver _configurationResolver;

        public DefaultBuilderContext(IConfigurationResolver configurationResolver)
        {
            _configurationResolver = configurationResolver;
        }

        public DefaultBuilderContext()
        {
            _configurationResolver = new DefaultConfigurationResolver();
        }

        public string MapPath(string relativePath)
        {
            Mandate.ParameterCondition(relativePath.StartsWith("~/"), "relativePath", "Must start with ~/");

            if (HostingEnvironment.IsHosted)
            {
                return HostingEnvironment.MapPath(relativePath);
            }

            return relativePath.Replace("~/", CurrentAssemblyDirectory + "/");
        }

        /// <summary>
        /// Gets the current assembly directory.
        /// </summary>
        /// <value>The assembly directory.</value>
        static public string CurrentAssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetCallingAssembly().CodeBase;
                var uri = new Uri(codeBase);
                var path = uri.LocalPath;
                return Path.GetDirectoryName(path);
            }
        }

        public IConfigurationResolver ConfigurationResolver { get { return _configurationResolver; } }
    }
}