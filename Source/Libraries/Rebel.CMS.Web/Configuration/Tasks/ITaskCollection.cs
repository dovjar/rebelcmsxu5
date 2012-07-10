using System.Collections.Generic;

namespace Rebel.Cms.Web.Configuration.Tasks
{
    public interface ITaskCollection
    {
        IEnumerable<ITask> Tasks { get; }
    }
}