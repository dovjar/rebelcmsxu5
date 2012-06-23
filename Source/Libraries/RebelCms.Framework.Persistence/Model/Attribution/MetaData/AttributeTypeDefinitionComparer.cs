using System.Collections.Generic;

namespace RebelCms.Framework.Persistence.Model.Attribution.MetaData
{
    public class AttributeTypeDefinitionComparer : IEqualityComparer<AttributeType>
    {
        #region Implementation of IEqualityComparer<in AttributeType>

        public bool Equals(AttributeType x, AttributeType y)
        {
            return x == y;
        }

        public int GetHashCode(AttributeType obj)
        {
            return obj.GetHashCode();
        }

        #endregion
    }
}