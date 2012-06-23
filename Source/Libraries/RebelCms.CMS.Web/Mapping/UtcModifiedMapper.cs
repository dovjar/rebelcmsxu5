using System;
using RebelCms.Cms.Web.Model.BackOffice;
using RebelCms.Framework.TypeMapping;

namespace RebelCms.Cms.Web.Mapping
{
    internal class UtcModifiedMapper : MemberMapper<TimestampedModel, DateTimeOffset>
    {
        public override DateTimeOffset GetValue(TimestampedModel source)
        {
            return source.UtcModified == default(DateTimeOffset) ? DateTimeOffset.UtcNow : source.UtcModified;
        }
    }
}