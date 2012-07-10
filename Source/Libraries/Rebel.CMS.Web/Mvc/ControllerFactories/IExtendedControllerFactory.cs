using System.Web.Mvc;
using Rebel.Cms.Web.Context;

namespace Rebel.Cms.Web.Mvc.ControllerFactories
{
    public interface IExtendedControllerFactory : IControllerFactory
    {
        /// <summary>
        /// Gets the application context.
        /// </summary>
        /// <remarks></remarks>
        IRebelApplicationContext ApplicationContext { get; }
    }
}