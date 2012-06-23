using RebelCms.Framework.Persistence.Model.Associations;

namespace RebelCms.Framework.Persistence.Abstractions
{
    public interface IRelatable
    {
        /// <summary>
        /// Gets relations for the current item.
        /// </summary>
        /// <remarks></remarks>
        EntityRelationCollection Relations { get; }
    }
}