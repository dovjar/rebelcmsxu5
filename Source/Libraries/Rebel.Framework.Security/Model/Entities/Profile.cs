using System;
using System.ComponentModel;
using Rebel.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Rebel.Framework.Persistence.Model.Constants.Schemas;
using Rebel.Framework.Persistence.ModelFirst;
using Rebel.Framework.Persistence.ModelFirst.Annotations;
using Rebel.Framework.Security.Model.Schemas;

namespace Rebel.Framework.Security.Model.Entities
{
    public class Profile : CustomTypedEntity, IProfile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Profile"/> class.
        /// </summary>
        public Profile()
        {
            ProviderUserKeyType = typeof(string);
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [AttributeAlias(Alias = NodeNameAttributeDefinition.AliasValue)]
        public string Name
        {
            get { return base.BaseAutoGetInner(NodeNameAttributeDefinition.AliasValue, "Name", string.Empty); }
            set { base.BaseAutoSetInner(NodeNameAttributeDefinition.AliasValue, "Name", value); }
        }

        /// <summary>
        /// Gets or sets the provider user key.
        /// </summary>
        /// <value>
        /// The provider user key.
        /// </value>
        [AttributeAlias(Alias = ProfileSchema.ProviderUserKeyAlias)]
        public virtual object ProviderUserKey
        {
            get
            {
                var val = base.BaseAutoGet<string>(ProfileSchema.ProviderUserKeyAlias, null);
                if (val == null)
                    return null;

                // Null checking
                var converter = TypeDescriptor.GetConverter(ProviderUserKeyType);
                return converter.ConvertFromString(val);
            }
            set
            {

                base.BaseAutoSet(ProfileSchema.ProviderUserKeyAlias, value == null ? null : value.ToString());
            }
        }

        /// <summary>
        /// Gets or sets the type of the provider user key.
        /// </summary>
        /// <value>
        /// The type of the provider user key.
        /// </value>
        public Type ProviderUserKeyType { get; private set; }

        /// <summary>
        /// Sets the type of the provider user key.
        /// </summary>
        /// <param name="type">The type.</param>
        internal void SetProviderUserKeyType(Type type)
        {
            ProviderUserKeyType = type;
        }
    }
}
