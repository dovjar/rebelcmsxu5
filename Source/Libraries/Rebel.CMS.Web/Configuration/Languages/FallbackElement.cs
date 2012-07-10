using System;
using System.Configuration;
using System.Runtime.Serialization;

namespace Rebel.Cms.Web.Configuration.Languages
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class FallbackElement : ConfigurationElement
    {
        /// <summary>
        /// Gets or sets the iso code.
        /// </summary>
        /// <value>
        /// The iso code.
        /// </value>
        [DataMember]
        [ConfigurationProperty("isoCode", IsRequired = true, IsKey = true)]
        public string IsoCode
        {
            get
            {
                return (string)this["isoCode"];
            }
            set
            {
                this["isoCode"] = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Configuration.ConfigurationElement"/> object is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Configuration.ConfigurationElement"/> object is read-only; otherwise, false.
        /// </returns>
        public override bool IsReadOnly()
        {
            return false;
        }
    }
}
