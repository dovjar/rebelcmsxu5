using System;
using System.Diagnostics.Contracts;
using Rebel.Framework.Data.Common;
using Rebel.Framework.EntityGraph.Domain.Versioning;

namespace Rebel.Framework.EntityGraph.Domain.Entity
{
    /// <summary>
    ///   The base of all navigable objects in RebelFramework
    /// </summary>
    [ContractClass(typeof (IEntityCodeContract))]
    public interface IEntity : ITracksConcurrency
    {
        /// <summary>
        ///   Gets or sets the status of the entity.
        /// </summary>
        /// <value>The status.</value>
        IEntityStatus Status { get; set; }

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
        IMappedIdentifier Id { get; set; }

        /// <summary>
        ///   Gets or sets the revision data.
        /// </summary>
        /// <value>The revision.</value>
        IRevisionData Revision { get; set; }
    }
}