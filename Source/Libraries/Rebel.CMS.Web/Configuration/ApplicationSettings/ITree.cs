using System;

namespace Rebel.Cms.Web.Configuration.ApplicationSettings
{
    public interface ITree
    {
        string ApplicationAlias { get; }
        Type ControllerType { get; }
    }
}