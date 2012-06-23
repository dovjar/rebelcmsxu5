using System;
using RebelCms.Framework;
using RebelCms.Framework.Context;

namespace RebelCms.Hive
{
    public interface IUnit : IRequiresFrameworkContext, IDisposable
    {
        /// <summary>
        /// Completes this unit of work.
        /// </summary>
        void Complete();

        /// <summary>
        /// Abandons this unit of work and its changes.
        /// </summary>
        void Abandon();

        /// <summary>
        /// Gets the unit-scoped cache.
        /// </summary>
        /// <value>The unit scoped cache.</value>
        AbstractScopedCache UnitScopedCache { get; }
    }
}