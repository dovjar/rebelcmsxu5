using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rebel.Tests.CoreAndFramework.Caching
{
    using System.Configuration;
    using NUnit.Framework;
    using Rebel.Framework;
    using Rebel.Framework.Caching;
    using Rebel.Framework.Configuration;
    using Rebel.Framework.Configuration.Caching;
    using Rebel.Framework.Data;
    using Rebel.Framework.Persistence.Model.Constants;
    using Rebel.Hive.Caching;
    using Rebel.Lucene.Caching;

    [TestFixture]
    public class CacheConfigFixture
    {
        [Test]
        public void CanGetProperties()
        {
            var cfg = (General)ConfigurationManager.GetSection(General.ConfigXmlKey);

            Assert.That(cfg.CacheProviders, Is.Not.Null);
            Assert.That(cfg.CacheProviders.ExtendedLifetime, Is.Not.Null);
            Assert.That(cfg.CacheProviders.LimitedLifetime, Is.Not.Null);

            var limitedType = cfg.CacheProviders.LimitedLifetime.GetProviderType();
            Assert.That(limitedType, Is.Not.Null);
            Assert.That(limitedType, Is.EqualTo(typeof (PerHttpRequestCacheProvider)));

            var extendedType = cfg.CacheProviders.ExtendedLifetime.GetProviderType();
            Assert.That(extendedType, Is.Not.Null);
            Assert.That(extendedType, Is.EqualTo(typeof(CacheProvider)));

            var testCacheKey = new HiveRelationCacheKey(HiveRelationCacheKey.RepositoryTypes.Entity,
                                                        HiveId.Empty,
                                                        Direction.Children,
                                                        FixedRelationTypes.DefaultRelationType);

            var checkGetPolicy = cfg.CacheProviders.ExtendedLifetime.GetPolicyElementFor(testCacheKey);
            Assert.That(checkGetPolicy, Is.Not.Null);
            Assert.That(checkGetPolicy.Name, Is.EqualTo("Standard"));
            Assert.That(checkGetPolicy.DurationSeconds, Is.EqualTo(3600));

            var testOtherCacheKey = new HiveRelationCacheKey(HiveRelationCacheKey.RepositoryTypes.Entity,
                                                        HiveId.Empty,
                                                        Direction.Children,
                                                        FixedRelationTypes.HostnameRelationType);

            checkGetPolicy = cfg.CacheProviders.ExtendedLifetime.GetPolicyElementFor(testOtherCacheKey);
            Assert.That(checkGetPolicy, Is.Not.Null);
            Assert.That(checkGetPolicy.Name, Is.EqualTo("NewOne"));
            Assert.That(checkGetPolicy.DurationSeconds, Is.EqualTo(5));

            // No rules should match this cache key so should just get the standard policy back
            var testAllOtherCacheKeys = new HiveRelationCacheKey(HiveRelationCacheKey.RepositoryTypes.Entity,
                                                        HiveId.Empty,
                                                        Direction.Children,
                                                        FixedRelationTypes.LanguageRelationType);

            checkGetPolicy = cfg.CacheProviders.ExtendedLifetime.GetPolicyElementFor(testAllOtherCacheKeys);
            Assert.That(checkGetPolicy, Is.Not.Null);
            Assert.That(checkGetPolicy.Name, Is.EqualTo("Standard"));
            Assert.That(checkGetPolicy.DurationSeconds, Is.EqualTo(3600));
        }

        [Test]
        public void WhenKeyShouldMatch_KeyMatchesIsTrue()
        {
            var rule = new CachePolicyPickerRule()
                {
                    ForKeyType = typeof (HiveRelationCacheKey).AssemblyQualifiedName,
                    Expression = "RelationType.RelationName == @0"
                };
            rule.Params.Add(new CachePolicyPickerRuleParameter()
            {
                Type = typeof(string).AssemblyQualifiedName,
                ValueAsString = "DefaultRelationType"
            });

            var testCacheKey = new HiveRelationCacheKey(HiveRelationCacheKey.RepositoryTypes.Entity,
                                                        HiveId.Empty,
                                                        Direction.Children,
                                                        FixedRelationTypes.DefaultRelationType);

            var matches = rule.KeyMatches(testCacheKey);

            Assert.True(matches);

            var mismatchingCacheKey = new HiveRelationCacheKey(HiveRelationCacheKey.RepositoryTypes.Entity,
                                                        HiveId.Empty,
                                                        Direction.Children,
                                                        FixedRelationTypes.HostnameRelationType);

            matches = rule.KeyMatches(mismatchingCacheKey);
            Assert.False(matches);

        }
    }
}
