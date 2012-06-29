namespace Umbraco.Framework.Dynamics
{
    #region Imports

    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Umbraco.Framework.Dynamics.Attributes;

    #endregion

    public class DynamicExtensionsHelper
    {
        private static readonly ConcurrentDictionary<ScannerCacheKey, IEnumerable<MethodInfo>> _methodCache =
            new ConcurrentDictionary<ScannerCacheKey, IEnumerable<MethodInfo>>();

        public static IEnumerable<MethodInfo> GetExtensions(IEnumerable<Assembly> assemblies,
                                                            IEnumerable<Type> supportedTypes)
        {
            return GetExtensions<DynamicExtensionAttribute>(assemblies, supportedTypes);
        }

        public static IEnumerable<MethodInfo> GetCachedExtensions<TAttributeType>(Assembly fromAssembly)
            where TAttributeType : Attribute
        {
            var cacheKey = new ScannerCacheKey
                {
                    AttributeScannedFor = typeof (TAttributeType),
                    ScannedAssembly = fromAssembly
                };

            var methods = _methodCache
                .GetOrAdd(cacheKey,
                          x =>
                          {
                              var withAttribute = TypeFinder.FindClassesWithAttribute<DynamicExtensionsAttribute>(fromAssembly.AsEnumerableOfOne(), false);
                              var allMethods = withAttribute.SelectMany(type => type.GetMethods());
                              var allMethodsWithAttribute = allMethods.Where(method => method.GetCustomAttributes<TAttributeType>(false).Any()).ToArray();
                              return allMethodsWithAttribute;
                          });
            return methods;
        }

        public static IEnumerable<MethodInfo> GetExtensions<TAttributeType>(IEnumerable<Assembly> assemblies,
                                                                            IEnumerable<Type> supportedTypes)
            where TAttributeType : Attribute
        {
            return assemblies.SelectMany(assembly =>
            {
                return GetCachedExtensions<TAttributeType>(assembly)
                    .Where(method =>
                    {
                        var args = method.GetParameters();
                        return args.Length > 0 && (supportedTypes.Contains(args[0].ParameterType));
                    });
            }).ToArray();
        }

        #region Nested type: ScannerCacheKey

        private class ScannerCacheKey : AbstractEquatableObject<ScannerCacheKey>
        {
            public Assembly ScannedAssembly { get; set; }
            public Type AttributeScannedFor { get; set; }

            protected override IEnumerable<PropertyInfo> GetMembersForEqualityComparison()
            {
                yield return this.GetPropertyInfo(x => x.ScannedAssembly);
                yield return this.GetPropertyInfo(x => x.AttributeScannedFor);
            }
        }

        #endregion
    }
}