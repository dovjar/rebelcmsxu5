using System;
using System.IO;
using System.Reflection;
using System.Web.Routing;
using NUnit.Framework;
using Rebel.Cms.Web.Configuration;
using Rebel.Framework;
using Rebel.Framework.DependencyManagement;
using Rebel.Cms.Web.DependencyManagement;
using Rebel.Cms.Web.Mvc.Areas;
using Rebel.Framework.DependencyManagement.Autofac;

namespace Rebel.Tests.Cms.Mvc.Routing
{
    [TestFixture]
    public class RegistrationViaAbstractedIoC
    {
        [Test]
        public void RebelAreaRegistrationReceivesMetadataFromIoC()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new Uri(codeBase);
            var path = uri.LocalPath;
            var binFolder = Path.GetDirectoryName(path);
            var settingsFile = new FileInfo(Path.Combine(binFolder, "web.config"));

            var autofacBuilder = new AutofacContainerBuilder();

            autofacBuilder.ForFactory(
                context => new RebelSettings(settingsFile))
                .KnownAsSelf();

            autofacBuilder.For<RebelAreaRegistration>()
                .KnownAsSelf()
                .WithResolvedParam(context => context.Resolve<IRouteHandler>("TreeRouteHandler"));

            //why is this here? 

            var typeFinder = new TypeFinder();
            var componentRegistrar = new RebelComponentRegistrar();
            componentRegistrar.RegisterEditorControllers(autofacBuilder, typeFinder);
            componentRegistrar.RegisterMenuItems(autofacBuilder, typeFinder);
            //componentRegistrar.RegisterPackageActions(autofacBuilder, typeFinder);
            componentRegistrar.RegisterPropertyEditors(autofacBuilder, typeFinder);
            componentRegistrar.RegisterSurfaceControllers(autofacBuilder, typeFinder);
            componentRegistrar.RegisterTreeControllers(autofacBuilder, typeFinder);

            //build the container
            var container = autofacBuilder.Build();

            var result = container.Resolve<RebelAreaRegistration>();

            Assert.IsNotNull(result);
        }
    }
}
