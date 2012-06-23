using System.Collections.Generic;

namespace RebelCms.Cms.Web.Configuration.ApplicationSettings
{
    public interface IApplicationCollection
    {
        IEnumerable<IApplication> Applications { get; }
    }
}