using System;

using RebelCms.Framework.Data.Common;
using RebelCms.Framework.DataManagement;
using RebelCms.Framework.Persistence.Abstractions.Versioning;

namespace RebelCms.Framework.Persistence.Abstractions
{
    /// <summary>
    ///   The base of all navigable objects in RebelCmsFramework
    /// </summary>
    public interface IPersistenceEntity : ITracksConcurrency
    {
        /// <summary>
        ///   Gets or sets the status of the entity.
        /// </summary>
        /// <value>The status.</value>
        IPersistenceEntityStatus Status { get; set; }

        /// <summary>
        ///   Gets or sets the creation date of the entity (UTC).
        /// </summary>
        /// <value>The UTC created.</value>
        DateTime UtcCreated { get; set; }

        /// <summary>
        ///   Gets or sets the modification date of the entity (UTC).
        /// </summary>
        /// <value>The UTC modified.</value>
        DateTime UtcModified { get; set; }

        /// <summary>
        ///   Gets or sets the timestamp when the status of this entity was last changed (in UTC).
        /// </summary>
        /// <value>The UTC status changed.</value>
        DateTime UtcStatusChanged { get; set; }

        /// <summary>
        ///   Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        HiveEntityUri Id { get; set; }

        /// <summary>
        ///   Gets or sets the revision data.
        /// </summary>
        /// <value>The revision.</value>
        IRevisionData Revision { get; set; }
    }
}