using System;

namespace Rebel.Framework.Persistence.Model.Associations
{
    using System.Runtime.Serialization;

    // TODO: Create immutable version of IReferenceByName and apply here
    [DataContract]
    public abstract class AbstractRelationType : AbstractEquatableObject<AbstractRelationType>
    {
        [DataMember]
        public abstract string RelationName { get; set; }

        protected override System.Collections.Generic.IEnumerable<System.Reflection.PropertyInfo> GetMembersForEqualityComparison()
        {
            yield return this.GetPropertyInfo(x => x.RelationName);
        }
    }
}