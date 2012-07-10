using Autofac;
using Rebel.Framework.EntityGraph.Domain.EntityAdaptors;

namespace Rebel.CMS.Kernel.AdaptorRepositories
{
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ContentAdaptor>()
                .As<IContentResolver>()
                .InstancePerLifetimeScope();

            base.Load(builder);
        }
    }
}
