using Autofac;
using RebelCms.Framework.EntityGraph.Domain.EntityAdaptors;

namespace RebelCms.CMS.Kernel.AdaptorRepositories
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
