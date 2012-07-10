using System.Collections.Generic;

namespace Rebel.Cms.Web.Configuration.ApplicationSettings
{
    public interface ITreeCollection
    {
        IEnumerable<ITree> Trees { get; }
    }
}