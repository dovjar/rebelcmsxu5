using System.Collections.Concurrent;

namespace Rebel.Framework.Dynamics
{
    /// <summary>
    /// A thread-safe collection of <see cref="BendyObject"/>
    /// </summary>
    /// <remarks></remarks>
    public class BendyObjectCollection : ConcurrentDictionary<string, BendyObject>
    {
        
    }
}