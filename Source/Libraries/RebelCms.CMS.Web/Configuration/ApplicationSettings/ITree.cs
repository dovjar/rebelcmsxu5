using System;

namespace RebelCms.Cms.Web.Configuration.ApplicationSettings
{
    public interface ITree
    {
        string ApplicationAlias { get; }
        Type ControllerType { get; }
    }
}