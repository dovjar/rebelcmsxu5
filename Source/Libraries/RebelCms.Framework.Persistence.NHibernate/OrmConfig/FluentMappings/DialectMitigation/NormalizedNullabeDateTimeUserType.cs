using System;

namespace RebelCms.Framework.Persistence.NHibernate.OrmConfig.FluentMappings.DialectMitigation
{
    public class NormalizedNullabeDateTimeUserType : NormalizedDateTimeUserType
    {
        public override Type ReturnedType
        {
            get { return typeof (DateTimeOffset?); }
        }
    }
}