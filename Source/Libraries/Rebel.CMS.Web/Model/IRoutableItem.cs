using System.Collections.Generic;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;

namespace Rebel.Cms.Web.Model
{
    /// <summary>
    /// The definition of a routable item
    /// </summary>
    public interface IRoutableItem
    {
        /// <summary>
        /// The ID of the Rebel model
        /// </summary>
        HiveId Id { get; }

        /// <summary>
        /// Gets the type of the content.
        /// </summary>
        /// <remarks></remarks>
        EntitySchema ContentType { get; }

        /// <summary>
        /// Gets the current template.
        /// </summary>
        /// <remarks></remarks>
        Template CurrentTemplate { get; }

        /// <summary>
        /// Gets or sets the alternative templates.
        /// </summary>
        /// <value>The alternative templates.</value>
        /// <remarks></remarks>
        IEnumerable<Template> AlternativeTemplates { get; }
    }
}
