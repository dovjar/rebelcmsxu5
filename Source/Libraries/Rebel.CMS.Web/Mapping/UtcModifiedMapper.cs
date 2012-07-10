using System;
using Rebel.Cms.Web.Model.BackOffice;
using Rebel.Framework.TypeMapping;

namespace Rebel.Cms.Web.Mapping
{
    internal class UtcModifiedMapper : MemberMapper<TimestampedModel, DateTimeOffset>
    {
        public override DateTimeOffset GetValue(TimestampedModel source)
        {
            return source.UtcModified == default(DateTimeOffset) ? DateTimeOffset.UtcNow : source.UtcModified;
        }
    }
}