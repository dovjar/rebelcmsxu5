using System;
using System.Collections;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using RebelCms.Framework;
using log4net.Config;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.DependencyManagement.DemandBuilders;
using RebelCms.Cms.Web.Mvc.Areas;
using RebelCms.Cms.Web.Mvc.ControllerFactories;
using RebelCms.Cms.Web.System.Boot;
using RebelCms.Cms.Web.Tasks;
using RebelCms.Cms.Web.Trees;
using RebelCms.Framework.DependencyManagement;
using RebelCms.Framework.DependencyManagement.Autofac;
using System.Collections.Generic;
using RebelCms.Framework.Diagnostics;
using IDependencyResolver = RebelCms.Framework.DependencyManagement.IDependencyResolver;
using RebelCms.Cms.Web.DependencyManagement;
using RebelCms.Framework.Tasks;

namespace RebelCms.Cms.Web.UI 
{
    using global::System.Threading;

    public class MvcApplication : HttpApplication
    {
        private static RebelCmsWebApplication _RebelCmsWebApplication;
        private static bool _isInitialised = false;
        private static readonly ReaderWriterLockSlim InitialiserLocker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        static MvcApplication()
        {
            LogHelper.TraceIfEnabled<MvcApplication>("Http application warmup");
        }

        /// <summary>
        /// Initializes the application
        /// </summary>
        protected virtual void Application_Start()
        {
            using (new WriteLockDisposable(InitialiserLocker))
            {
                if (_isInitialised) return;
                _RebelCmsWebApplication = CreateRebelCmsApplication();
                _RebelCmsWebApplication.Start();
                _isInitialised = true;
            }
        }

        // RebelCmsWebApplication cannot hook this event programatically in the Start event due to a known
        // bug in ASP.NET causing a NullRef in PipelineRequestManager if events are hooked in the application init (http://forums.asp.net/t/1327102.aspx/1)
        protected virtual void Application_EndRequest(object sender, EventArgs e)
        {
            _RebelCmsWebApplication.IfNotNull(x => x.OnEndRequest(sender, e));
        }

        /// <summary>
        /// Creates the container builder. Override this if you'd like to use an alternative <see cref="AbstractContainerBuilder"/> to the default.
        /// </summary>
        /// <returns></returns>
        protected virtual AbstractContainerBuilder CreateContainerBuilder()
        {
            return new AutofacContainerBuilder();
        }

        protected virtual Func<IDependencyResolver, global::System.Web.Mvc.IDependencyResolver> MvcResolverFactory()
        {
            return x => new AutofacMvcResolver(x);
        }

        /// <summary>
        /// Creates the RebelCms application. Override this if you'd like to use your own extended version of <see cref="RebelCmsWebApplication"/>.
        /// </summary>
        /// <returns></returns>
        protected virtual RebelCmsWebApplication CreateRebelCmsApplication()
        {
            return new RebelCmsWebApplication(this, CreateContainerBuilder(), MvcResolverFactory());
        }
    }
}