using System.Web.Mvc;
using RebelCms.Cms.Web.Context;

namespace RebelCms.Cms.Web.Mvc.ControllerFactories
{
    public interface IExtendedControllerFactory : IControllerFactory
    {
        /// <summary>
        /// Gets the application context.
        /// </summary>
        /// <remarks></remarks>
        IRebelCmsApplicationContext ApplicationContext { get; }
    }
}