using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.IO;
using System.Web.Security;
using Rebel.Cms.Web.Configuration.ApplicationSettings;
using Rebel.Cms.Web.Configuration.Dashboards;
using Rebel.Cms.Web.Configuration.Languages;
using Rebel.Cms.Web.Configuration.Tasks;
using Rebel.Cms.Web.Configuration.RebelSystem;
using Rebel.Framework;
using Rebel.Framework.Configuration;
using Rebel.Framework.Security.Configuration;

namespace Rebel.Cms.Web.Configuration
{
    /// <summary>
    /// Contains collections of all Rebel configuration settings
    /// </summary>
    public class RebelSettings
    {

        public const string GroupXmlKey = "rebel.cms";

        public static RebelSettings GetSettings()
        {
            var settings = new RebelSettings();
            return settings;
        }

        private RebelSettings()
        {
            _rebelSystem = (RebelSystemConfiguration)ConfigurationManager
                .GetSection(RebelSystemConfiguration.ConfigXmlKey);
        }

        internal RebelSettings(FileInfo configFile)
        {
            Mandate.That(configFile.Exists, x => new ArgumentException("The configuration file specified doesn't exist ({0})".InvariantFormat(configFile.FullName), "configFile"));

            var fileMap = new ExeConfigurationFileMap { ExeConfigFilename = configFile.FullName };
            var configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

            _rebelSystem = (RebelSystemConfiguration)configuration
                .GetSection(RebelSystemConfiguration.ConfigXmlKey);
        }

        private readonly RebelSystemConfiguration _rebelSystem;

        #region RebelSettings Members

        public PluginManagerConfiguration PluginConfig
        {
            get { return PluginManagerConfiguration.GetSettings(); }
        }

        /// <summary>
        /// Returns all rebel applications configured using the deep config manager
        /// </summary>
        public IEnumerable<IApplication> Applications
        {
            get
            {                               
                var appConfig = DeepConfigManager.Default
                    .GetPluginSettings<ApplicationConfiguration, IApplicationCollection>(ApplicationConfiguration.ConfigXmlKey, x => x);

                if (appConfig != null)
                {
                    //only return unique applications
                    var apps = appConfig.SelectMany(x => x.Applications).DistinctBy(x => x.Alias).OrderBy(x => x.Ordinal).ToArray();

                    return apps;
                }
                return Enumerable.Empty<ApplicationElement>();
                
            }
        }


        public IEnumerable<IDashboardGroup> DashboardGroups
        {
            get
            {
                //get the dashboard groups from the deep config manager
                var dashboardConfig = DeepConfigManager.Default
                    .GetPluginSettings<DashboardGroupConfiguration, IDashboardConfig>(DashboardGroupConfiguration.ConfigXmlKey, x => x);
                
                if (dashboardConfig != null)
                {
                    return dashboardConfig.SelectMany(x => x.Groups);
                }
                return Enumerable.Empty<IDashboardGroup>();
            }
        }

        /// <summary>
        /// Returns all rebel applications configured using the deep config manager
        /// </summary>
        public IEnumerable<ITree> Trees
        {
            get
            {

                //get the trees from the deep config manager
                var treeConfig = DeepConfigManager.Default
                    .GetPluginSettings<TreeConfiguration, ITreeCollection>(TreeConfiguration.ConfigXmlKey, x => x);
                if (treeConfig != null)
                {                    
                    //Only return unique trees, trees are unique to a combo of app alias and controller type name
                    var trees = treeConfig.SelectMany(x => x.Trees).Distinct(
                        new DeferredEqualityComparer<ITree>((t1, t2) => 
                            t1.ApplicationAlias == t2.ApplicationAlias && t1.ControllerType == t2.ControllerType, 
                            x => (x.ApplicationAlias + x.ControllerType.GetHashCode()).GetHashCode()));

                    return trees;
                }
                return Enumerable.Empty<ITree>();
            }
        }

        /// <summary>
        /// Returns all languages configured using the deep config manager
        /// </summary>
        public IEnumerable<LanguageElement> Languages
        {
            get
            {
                //get the languages from the deep config manager
                var languagesConfig = DeepConfigManager.Default
                    .GetPluginSettings<LanguagesConfiguration, LanguagesConfiguration>(LanguagesConfiguration.ConfigXmlKey, x => x);
                if (languagesConfig != null)
                {
                    return languagesConfig.SelectMany(x => x.Languages.Cast<LanguageElement>());
                }
                return Enumerable.Empty<LanguageElement>();
            }
        }

        /// <summary>
        /// Returns all tasks registered in config
        /// </summary>
        public IEnumerable<ITask> Tasks
        {
            get
            {
                //get the tasks from the deep config manager
                var taskConfig = DeepConfigManager.Default
                    .GetPluginSettings<TasksConfiguration, TasksConfiguration>(TasksConfiguration.ConfigXmlKey, x => x);
                if (taskConfig != null)
                {
                    var tasks = taskConfig.SelectMany(x => x.Tasks.OnlyLocalConfig<TaskElement>()).ToArray();
                    var packagePath = (PluginConfig.PluginsPath + "/Packages").Replace("/", "\\").TrimStart('~').TrimEnd('\\');
                    //now, we need to assign the package to the task if the task was found in a package)))
                    foreach (var obj in from t in tasks 
                                      let configFilePath = new FileInfo(t.ElementInformation.Source) 
                                      where configFilePath.Directory.Parent != null
                                      let parentFolder = configFilePath.Directory.Parent
                                      where parentFolder.FullName.TrimEnd('\\').EndsWith(packagePath, StringComparison.InvariantCultureIgnoreCase) 
                                      select new {Task = t, Folder = configFilePath.Directory})
                    {
                        obj.Task.PackageFolder = obj.Folder.FullName;
                    }

                    return tasks;
                }
                return Enumerable.Empty<ITask>();

            }
        }

        public RebelUrlsElement Urls
        {
            get { return _rebelSystem.Urls; }
        }

        public PackageRepositoryElement PublicPackageRepository
        {
            get { return _rebelSystem.PublicPackageRepository; }
        }

        public RebelFoldersElement RebelFolders
        {
            get { return _rebelSystem.Folders; }
        }

        public RebelPathsElement RebelPaths
        {
            get { return _rebelSystem.Paths; }
        }

        public IEnumerable<RouteMatchElement> RouteMatches
        {
            get { return _rebelSystem.RouteMatches; }
        }

        public IEnumerable<MembershipProviderElement> MembershipProviders
        {
            get
            {
                var securityConfig = DeepConfigManager.Default
                    .GetPluginSettings<SecurityConfiguration, SecurityConfiguration>(SecurityConfiguration.ConfigXmlKey, x => x);
                if (securityConfig != null)
                {
                    return securityConfig.SelectMany(x => x.MembershipProviders.Cast<MembershipProviderElement>());
                }
                return Enumerable.Empty<MembershipProviderElement>();
            }
        }

        public string BackofficeCookieName
        {
            get
            {
                var securityConfig = DeepConfigManager.Default
                    .GetPluginSettings<SecurityConfiguration, SecurityConfiguration>(SecurityConfiguration.ConfigXmlKey, x => x);

                if (securityConfig != null)
                {
                    var backOfficeCookieSettings = securityConfig.Select(x => x.BackofficeCookie).LastOrDefault();
                    if (backOfficeCookieSettings != null)
                    {
                        return backOfficeCookieSettings.Name;
                    }
                }

                return ".UMBAUTH";
            }
        }

        public string BackofficeCookieDomain
        {
            get
            {
                var securityConfig = DeepConfigManager.Default
                    .GetPluginSettings<SecurityConfiguration, SecurityConfiguration>(SecurityConfiguration.ConfigXmlKey, x => x);

                if (securityConfig != null)
                {
                    var backOfficeCookieSettings = securityConfig.Select(x => x.BackofficeCookie).LastOrDefault();
                    if(backOfficeCookieSettings != null && backOfficeCookieSettings.DomainType == BackofficeCookieDomainType.Custom)
                    {
                        return backOfficeCookieSettings.Domain;
                    }
                }

                return FormsAuthentication.CookieDomain;
            }
        }

        #endregion
    }
}
