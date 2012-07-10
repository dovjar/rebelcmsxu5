namespace Rebel.Framework.Configuration.Caching
{
    using System;
    using System.Collections.Generic;

    public class ExtendedLifetime : LimitedLifetime
    {
        public override Type GetKnownDefaultProviderType()
        {
            return null;
        }

        public override IEnumerable<string> GetAlternativeDefaultProviderTypes()
        {
            yield return "Rebel.Lucene.Caching.CacheProvider, Rebel.Lucene";
        }
    }
}