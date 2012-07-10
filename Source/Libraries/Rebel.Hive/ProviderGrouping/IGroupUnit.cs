using System;
using Rebel.Framework;
using Rebel.Framework.Context;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Hive.ProviderGrouping
{
    public interface IGroupUnit<out TFilter> : IDisposable, IUnit
        where TFilter : class, IProviderTypeFilter
    {
        Uri IdRoot { get; }
        IEntityRepositoryGroup<TFilter> Repositories { get; }
        bool WasAbandoned { get; }
    }
}