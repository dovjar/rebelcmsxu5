using System.Collections.Generic;
using System.Configuration;
using RebelCms.Framework;

namespace RebelCms.Cms.Web.Configuration.Tasks
{


    public class TasksConfiguration : ConfigurationSection, ITaskCollection
    {

        public const string ConfigXmlKey = RebelCmsSettings.GroupXmlKey + "/tasks";

        [ConfigurationCollection(typeof(TasksCollection))]
        [ConfigurationProperty("", IsDefaultCollection = true, IsRequired = true)]
        public TasksCollection Tasks
        {
            get
            {
                return (TasksCollection)base[""];
            }
        }


        IEnumerable<ITask> ITaskCollection.Tasks
        {
            get { return Tasks.OnlyLocalConfig<ITask>(); }
        }
    }
}
