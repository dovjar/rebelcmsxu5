using RebelCms.Cms.Web.Context;

namespace RebelCms.Cms.Web.Tasks
{
    public abstract class ConfigurationTask : AbstractWebTask
    {
        public ConfigurationTaskContext ConfigurationTaskContext { get; private set; }

        protected ConfigurationTask(ConfigurationTaskContext configurationTaskContext)
            : base(configurationTaskContext.ApplicationContext)
        {
            ConfigurationTaskContext = configurationTaskContext;
        }
    }
}