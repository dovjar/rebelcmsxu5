using System.Collections.Generic;
using System.Configuration;
using Rebel.Cms.Web.Configuration.Tasks;
using System.Linq;

namespace Rebel.Cms.Web.Context
{
    public class ConfigurationTaskContext
    {
        public ConfigurationTaskContext(IRebelApplicationContext applicationContext, IEnumerable<ITaskParameter> parameters, ITask task)
        {
            Parameters = parameters.ToDictionary(x => x.Name, x => x.Value);
            Task = task;
            ApplicationContext = applicationContext;
        }

        public IRebelApplicationContext ApplicationContext { get; private set; }

        public IDictionary<string, string> Parameters { get; private set; }

        /// <summary>
        /// The task used to construct the context
        /// </summary>
        public ITask Task { get; private set; }
    }
}