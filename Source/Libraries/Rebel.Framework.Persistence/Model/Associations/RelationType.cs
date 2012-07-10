using System;

namespace Rebel.Framework.Persistence.Model.Associations
{
    using System.Runtime.Serialization;

    [Serializable]
    [DataContract]
    public class RelationType : AbstractRelationType
    {
        public RelationType()
        {}

        private string _relationName = string.Empty;

        public RelationType(string relationName)
        {
            _relationName = relationName;
        }

        [DataMember]
        public override string RelationName
        {
            get { return _relationName; }
            set { _relationName = value; }
        }
    }
}