using System;
using System.IO;
using System.Reflection;
using System.Web.Routing;
using NUnit.Framework;
using RebelCms.Cms.Web.Configuration;
using RebelCms.Framework;
using RebelCms.Framework.DependencyManagement;
using RebelCms.Cms.Web.DependencyManagement;
using RebelCms.Cms.Web.Mvc.Areas;
using RebelCms.Framework.DependencyManagement.Autofac;

namespace RebelCms.Tests.Cms.Mvc.Routing
{
    [TestFixture]
    public class RegistrationViaAbstractedIoC
    {
        [Test]
        public void RebelCmsAreaRegistrationReceivesMetadataFromIoC()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new Uri(codeBase);
            var path = uri.LocalPath;
            var binFolder = Path.GetDirectoryName(path);
            var settingsFile = new FileInfo(Path.Combine(binFolder, "web.config"));

            var autofacBuilder = new AutofacContainerBuilder();

            autofacBuilder.ForFactory(
                context => new RebelCmsSettings(settingsFile))
                .KnownAsSelf();

            autofacBuilder.For<RebelCmsAreaRegistration>()
                .KnownAsSelf()
                .WithResolvedParam(context => context.Resolve<IRouteHandler>("TreeRouteHandler"));

            //why is this here? 

            var typeFinder = new TypeFinder();
            var componentRegistrar = new RebelCmsComponentRegistrar();
            componentRegistrar.RegisterEditorControllers(autofacBuilder, typeFinder);
            componentRegistrar.RegisterMenuItems(autofacBuilder, typeFinder);
            //componentRegistrar.RegisterPackageActions(autofacBuilder, typeFinder);
            componentRegistrar.RegisterPropertyEditors(autofacBuilder, typeFinder);
            componentRegistrar.RegisterSurfaceControllers(autofacBuilder, typeFinder);
            componentRegistrar.RegisterTreeControllers(autofacBuilder, typeFinder);

            //build the container
            var container = autofacBuilder.Build();

            var result = container.Resolve<RebelCmsAreaRegistration>();

            Assert.IsNotNull(result);
        }
    }
}
