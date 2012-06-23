using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.IO;
using RebelCms.Cms.Web.Configuration.ApplicationSettings;
using RebelCms.Cms.Web.Configuration.Dashboards;
using RebelCms.Cms.Web.Configuration.Languages;
using RebelCms.Cms.Web.Configuration.Tasks;
using RebelCms.Cms.Web.Configuration.RebelCmsSystem;
using RebelCms.Framework;
using RebelCms.Framework.Configuration;

namespace RebelCms.Cms.Web.Configuration
{
    /// <summary>
    /// Contains collections of all RebelCms configuration settings
    /// </summary>
    public class RebelCmsSettings
    {

        public const string GroupXmlKey = "RebelCms.cms";

        public static RebelCmsSettings GetSettings()
        {
            var settings = new RebelCmsSettings();
            return settings;
        }

        private RebelCmsSettings()
        {
            _RebelCmsSystem = (RebelCmsSystemConfiguration)ConfigurationManager
                .GetSection(RebelCmsSystemConfiguration.ConfigXmlKey);
        }

        internal RebelCmsSettings(FileInfo configFile)
        {
            Mandate.That(configFile.Exists, x => new ArgumentException("The configuration file specified doesn't exist ({0})".InvariantFormat(configFile.FullName), "configFile"));

            var fileMap = new ExeConfigurationFileMap { ExeConfigFilename = configFile.FullName };
            var configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

            _RebelCmsSystem = (RebelCmsSystemConfiguration)configuration
                .GetSection(RebelCmsSystemConfiguration.ConfigXmlKey);
        }

        private readonly RebelCmsSystemConfiguration _RebelCmsSystem;

        #region RebelCmsSettings Members

        public PluginManagerConfiguration PluginConfig
        {
            get { return PluginManagerConfiguration.GetSettings(); }
        }

        /// <summary>
        /// Returns all RebelCms applications configured using the deep config manager
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
        /// Returns all RebelCms applications configured using the deep config manager
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

        public RebelCmsUrlsElement Urls
        {
            get { return _RebelCmsSystem.Urls; }
        }

        public PublicPackageRepositoryElement PublicPackageRepository
        {
            get { return _RebelCmsSystem.PublicPackageRepository; }
        }

        public RebelCmsFoldersElement RebelCmsFolders
        {
            get { return _RebelCmsSystem.Folders; }
        }

        public RebelCmsPathsElement RebelCmsPaths
        {
            get { return _RebelCmsSystem.Paths; }
        }

        public IEnumerable<RouteMatchElement> RouteMatches
        {
            get { return _RebelCmsSystem.RouteMatches; }
        }

        #endregion
    }
}
