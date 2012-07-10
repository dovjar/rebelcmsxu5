using System;
using System.Collections.Generic;

namespace Rebel.Cms.Web.Configuration.Tasks
{
    public interface ITask
    {
        Type TaskType { get; }
        string Trigger { get; }
        string PackageFolder { get; set; }
        IEnumerable<ITaskParameter> Parameters { get; } 
    }
}