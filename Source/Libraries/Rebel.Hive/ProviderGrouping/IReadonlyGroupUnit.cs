using System;
using Rebel.Framework.Context;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Hive.ProviderGrouping
{
    public interface IReadonlyGroupUnit<out TFilter> : IDisposable, IUnit, IRequiresFrameworkContext
        where TFilter : class, IProviderTypeFilter
    {
        Uri IdRoot { get; }
        IReadonlyEntityRepositoryGroup<TFilter> Repositories { get; }
    }
}