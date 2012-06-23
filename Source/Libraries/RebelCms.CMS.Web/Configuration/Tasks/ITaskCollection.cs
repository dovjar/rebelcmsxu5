using System.Collections.Generic;

namespace RebelCms.Cms.Web.Configuration.Tasks
{
    public interface ITaskCollection
    {
        IEnumerable<ITask> Tasks { get; }
    }
}