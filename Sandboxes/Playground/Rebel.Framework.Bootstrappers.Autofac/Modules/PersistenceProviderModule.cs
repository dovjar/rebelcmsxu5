using System.Diagnostics.Contracts;
using Autofac;
using Rebel.Framework.Persistence;

namespace Rebel.Framework.Bootstrappers.Autofac.Modules
{
  public class PersistenceProviderModule : Module
  {
    protected override void Load(ContainerBuilder builder)
    {
      Contract.Assert(builder != null, "Builder container is null");

      // Register a singleton PersistenceProviderCollection
      builder.RegisterType<PersistenceProviderCollection>().As<PersistenceProviderCollection>().SingleInstance();
    }
  }
}