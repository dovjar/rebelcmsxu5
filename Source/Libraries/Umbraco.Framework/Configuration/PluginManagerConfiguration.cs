using System;
using System.Configuration;

namespace Umbraco.Framework.Configuration
{
    public enum PluginManagerShadowCopyType
    {
        /// <summary>
        /// Uses the ~/bin folder
        /// </summary>
        Default,

        /// <summary>
        /// Uses the CodeGen folder, only works in Full Trust
        /// </summary>
        UseDynamicFolder,

        /// <summary>
        /// Uses the specified folder in config
        /// </summary>
        OverrideDefaultFolder
    }

    /// <summary>
    /// Configuration 
    /// </summary>
    public class PluginManagerConfiguration : ConfigurationSection
    {
        public const string ConfigXmlKey = "umbraco/pluginManager";

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <returns></returns>
        public static PluginManagerConfiguration GetSettings()
        {
            var mgr = (PluginManagerConfiguration)ConfigurationManager.GetSection(ConfigXmlKey);
            return mgr;
        }
        
        /// <summary>
        /// The Pluginmanager xml configuration key
        /// </summary>
        public const string PluginManagerXmlKey = "pluginManager";
        private const string ShadowCopyFolderXmlKey = "shadowCopyFolder";
        private const string PluginsFolderXmlKey = "pluginsFolder";
        private const string ShadowCopyTypeKey = "shadowCopyType";

        /// <summary>
        /// Gets or sets the plugins path.
        /// </summary>
        /// <value>
        /// The plugins path.
        /// </value>
        [ConfigurationProperty(PluginsFolderXmlKey, DefaultValue = "~/App_Plugins")]
        public string PluginsPath
        {
            get { return this[PluginsFolderXmlKey].ToString(); }
            set
            {
                this[PluginsFolderXmlKey] = value;
            }
        }

        /// <summary>
        /// Determines the shadow copy type
        /// </summary>
        [ConfigurationProperty(ShadowCopyTypeKey, DefaultValue = "Default")]
        public string ShadowCopyTypeAsString
        {
            get { return (string)this[ShadowCopyTypeKey]; }
            set
            {
                this[ShadowCopyTypeKey] = value;
            }
        }

        /// <summary>
        /// Returns the shadow copy type to use
        /// </summary>
        public PluginManagerShadowCopyType ShadowCopyType
        {
            get
            {
                PluginManagerShadowCopyType type;
                if (Enum.TryParse(ShadowCopyTypeAsString, true, out type))
                {
                    return type;
                }
                throw new InvalidOperationException("Could not parse value " + ShadowCopyTypeAsString + " into enum " + typeof(PluginManagerShadowCopyType).FullName);
            }            
        }

        /// <summary>
        /// Gets or sets the shadow copy path, if this is not set to /bin then you MUST have probing paths
        /// setup in you web.config properly.
        /// </summary>
        /// <value>
        /// The shadow copy path.
        /// </value>
        /// <remarks>
        /// This value doesn't affect the plugin manager if running in full trust and UseDynamicFolder is set to true
        /// </remarks>
        [ConfigurationProperty(ShadowCopyFolderXmlKey, DefaultValue = "~/bin")]
        public string ShadowCopyPath
        {
            get { return this[ShadowCopyFolderXmlKey].ToString(); }
            set
            {
                this[ShadowCopyFolderXmlKey] = value;
            }
        }

    }
}