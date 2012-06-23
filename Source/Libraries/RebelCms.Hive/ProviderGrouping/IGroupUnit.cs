using System;
using RebelCms.Framework;
using RebelCms.Framework.Context;
using RebelCms.Hive.RepositoryTypes;

namespace RebelCms.Hive.ProviderGrouping
{
    public interface IGroupUnit<out TFilter> : IDisposable, IUnit
        where TFilter : class, IProviderTypeFilter
    {
        Uri IdRoot { get; }
        IEntityRepositoryGroup<TFilter> Repositories { get; }
        bool WasAbandoned { get; }
    }
}