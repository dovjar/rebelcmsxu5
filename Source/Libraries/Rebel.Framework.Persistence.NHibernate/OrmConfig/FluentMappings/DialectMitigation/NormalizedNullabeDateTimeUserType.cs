using System;

namespace Rebel.Framework.Persistence.NHibernate.OrmConfig.FluentMappings.DialectMitigation
{
    [Serializable]
    public class NormalizedNullabeDateTimeUserType : NormalizedDateTimeUserType
    {
        public override Type ReturnedType
        {
            get { return typeof (DateTimeOffset?); }
        }
    }
}