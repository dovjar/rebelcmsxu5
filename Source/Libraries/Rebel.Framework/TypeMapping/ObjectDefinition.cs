using System;

namespace Rebel.Framework.TypeMapping
{
    /// <summary>
    /// Defines an object
    /// </summary>
    public class ObjectDefinition
    {
        public Type Type { get; set; }

        public object Value { get; set; }
    }
}