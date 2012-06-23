using System.Collections.Generic;
using System.Configuration;
using RebelCms.Cms.Web.Configuration.Tasks;
using System.Linq;

namespace RebelCms.Cms.Web.Context
{
    public class ConfigurationTaskContext
    {
        public ConfigurationTaskContext(IRebelCmsApplicationContext applicationContext, IEnumerable<ITaskParameter> parameters, ITask task)
        {
            Parameters = parameters.ToDictionary(x => x.Name, x => x.Value);
            Task = task;
            ApplicationContext = applicationContext;
        }

        public IRebelCmsApplicationContext ApplicationContext { get; private set; }

        public IDictionary<string, string> Parameters { get; private set; }

        /// <summary>
        /// The task used to construct the context
        /// </summary>
        public ITask Task { get; private set; }
    }
}