using System.Collections.Generic;

namespace Rebel.Framework.Security
{
    public class PermissionMetadata : PluginMetadataComposition
    {
        public PermissionMetadata(IDictionary<string, object> obj)
            : base(obj)
        { }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the user type.
        /// </summary>
        /// <value>
        /// The user type.
        /// </value>
        public UserType UserType { get; set; }
    }
}
