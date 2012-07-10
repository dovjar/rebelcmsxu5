using System.Collections.Generic;

namespace Rebel.Cms.Web.Configuration.ApplicationSettings
{
    public interface IApplicationCollection
    {
        IEnumerable<IApplication> Applications { get; }
    }
}