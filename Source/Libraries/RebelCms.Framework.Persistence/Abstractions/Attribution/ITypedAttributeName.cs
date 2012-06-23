using System;
using RebelCms.Foundation;

namespace RebelCms.Framework.Persistence.Abstractions.Attribution
{
    /// <summary>
    ///   Represents the name of an <see cref = "ITypedAttribute" />
    /// </summary>
    public interface ITypedAttributeName : IReferenceByAlias, IComparable<ITypedAttributeName>
    {
    }
}