using System.Collections.Generic;

namespace RebelCms.Cms.Web.Configuration.ApplicationSettings
{
    public interface ITreeCollection
    {
        IEnumerable<ITree> Trees { get; }
    }
}