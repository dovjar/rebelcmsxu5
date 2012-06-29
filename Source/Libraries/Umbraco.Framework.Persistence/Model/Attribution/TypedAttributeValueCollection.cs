using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;

using Umbraco.Framework.Persistence.Abstractions.Attribution.MetaData;

namespace Umbraco.Framework.Persistence.Model.Attribution
{
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Threading;

    [DebuggerDisplay("{ToDebugString()}")]
    [Serializable]
    [DataContract(IsReference = true)]
    public class TypedAttributeValueCollection : NotifyingDictionary<string, object>
    {
        /// <summary>
        /// The key is "Value" by default because this is the default property name of PropertyEditors and serialization
        /// will just use the property names.
        /// </summary>
        public const string DefaultAttributeValueKey = "Value";

        public virtual object GetDefaultValue()
        {
            object retrieve = null;
            return TryGetValue(DefaultAttributeValueKey, out retrieve) ? retrieve : null;
        }

        public virtual void SetDefaultValue(object value)
        {
            AddOrUpdate(DefaultAttributeValueKey, value);
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}