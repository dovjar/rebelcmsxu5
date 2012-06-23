using System;
using RebelCms.Framework.Context;
using RebelCms.Hive.RepositoryTypes;

namespace RebelCms.Hive.ProviderGrouping
{
    public interface IReadonlyGroupUnit<out TFilter> : IDisposable, IUnit, IRequiresFrameworkContext
        where TFilter : class, IProviderTypeFilter
    {
        Uri IdRoot { get; }
        IReadonlyEntityRepositoryGroup<TFilter> Repositories { get; }
    }
}