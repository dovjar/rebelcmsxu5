using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Umbraco.Cms.Web.Model;
using Umbraco.Framework;
using Umbraco.Framework.Dynamics;
using Umbraco.Framework.Dynamics.Attributes;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Attribution;

namespace Umbraco.Cms.Web
{
    using global::System.Collections.Concurrent;

    public class BendyObjectExtensionsHelper
    {
        /// <summary>
        /// Applies the dynamic extensions.
        /// </summary>
        /// <param name="bendyObject">The bendy object.</param>
        public static void ApplyDynamicExtensions<TItemType>(BendyObject bendyObject, IEnumerable<Assembly> dynamicExtensionAssemblies = null)
        {
            IEnumerable<MethodInfo> methods;
            var supportedTypes = new[]
                {
                    typeof (BendyObject), typeof (TItemType)
                };

            if (dynamicExtensionAssemblies == null)
                dynamicExtensionAssemblies = TypeFinder.GetFilteredDomainAssemblies().ToArray();

            methods = DynamicExtensionsHelper.GetExtensions(dynamicExtensionAssemblies, supportedTypes);

            ApplyDynamicExtensions<TItemType>(bendyObject, methods);
        }

        private static ConcurrentDictionary<string, DynamicMethod> _applyableDynamicExtensionsCache = new ConcurrentDictionary<string, DynamicMethod>();

        /// <summary>
        /// Applies the dynamic extensions.
        /// </summary>
        /// <param name="bendyObject">The bendy object.</param>
        /// <param name="extentionMethods">The extention methods.</param>
        public static void ApplyDynamicExtensions<TItemType>(BendyObject bendyObject, IEnumerable<MethodInfo> extentionMethods)
        {
            var typeKey = typeof (TItemType).FullName;
            foreach (var method in extentionMethods)
            {
                var methodKey = method.DeclaringType.FullName + method.Name + TypeExtensions.GetCacheKeyFromParameters(method.GetParameters());
                MethodInfo methodCopied = method;

                var dynamicMeth = _applyableDynamicExtensionsCache
                    .GetOrAdd(typeKey + methodKey, key =>
                    {
                        var attrib = methodCopied.GetCustomAttributes<DynamicExtensionAttribute>(false).FirstOrDefault();

                        if (attrib == null) return null;

                        var parameters = methodCopied.GetParameters();

                        var expressionParams = parameters.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();
                        var expression = Expression.Call(methodCopied, expressionParams.Cast<Expression>());

                        var sig = new Signature(!string.IsNullOrWhiteSpace(attrib.Name) ? attrib.Name : methodCopied.Name, methodCopied.ReturnType, parameters.Select(x => new Parameter(x.Name, x.ParameterType)).ToArray());
                        var dynamicMethod = new DynamicMethod(sig, Expression.Lambda(expression, expressionParams),
                            parameters[0].ParameterType == typeof(TItemType) ? typeof(TItemType) : null);
                        return dynamicMethod;
                    });

                if (dynamicMeth != null) bendyObject.AddMethod(dynamicMeth);
            }
        }

        /// <summary>
        /// Applies the dynamic field extensions.
        /// </summary>
        /// <typeparam name="TParentType">The type of the parent item.</typeparam>
        /// <param name="parent">The parent.</param>
        /// <param name="propertyEditorId">The property editor id.</param>
        /// <param name="bendyObject">The bendy object.</param>
        public static void ApplyDynamicFieldExtensions<TParentType>(TParentType parent, string propertyEditorId, BendyObject bendyObject, IEnumerable<Assembly> dynamicExtensionAssemblies = null)
            where TParentType : TypedEntity
        {
            IEnumerable<MethodInfo> methods;
            var supportedTypes = new[]
                {
                    typeof(BendyObject),
                typeof(TypedAttribute),
                typeof(TParentType)
                };

            if (dynamicExtensionAssemblies == null)
                dynamicExtensionAssemblies = TypeFinder.GetFilteredDomainAssemblies().ToArray();

            methods = DynamicExtensionsHelper.GetExtensions<DynamicFieldExtensionAttribute>(dynamicExtensionAssemblies, supportedTypes);

            ApplyDynamicFieldExtensions(parent, propertyEditorId, bendyObject, methods);
        }

        /// <summary>
        /// Applies the dynamic field extensions.
        /// </summary>
        /// <typeparam name="TParentType">The type of the parent item.</typeparam>
        /// <param name="parent">The parent.</param>
        /// <param name="propertyEditorId">The property editor id.</param>
        /// <param name="bendyObject">The bendy object.</param>
        /// <param name="extentionMethods">The extention methods.</param>
        public static void ApplyDynamicFieldExtensions<TParentType>(TParentType parent, string propertyEditorId, BendyObject bendyObject, IEnumerable<MethodInfo> extentionMethods)
            where TParentType : TypedEntity
        {
            Guid propEdIdasGuid = Guid.Empty;
            if (!propertyEditorId.IsNullOrWhiteSpace() && !Guid.TryParse(propertyEditorId, out propEdIdasGuid))
            {
                throw new InvalidCastException("The propertyEditorId specified could not be parsed as a valid GUID");
            }
            foreach (var method in extentionMethods)
            {
                var attribs = method.GetCustomAttributes<DynamicFieldExtensionAttribute>(false);
                foreach (var attrib in attribs.Where(x =>
                    {
                        Guid id;
                        return Guid.TryParse(x.PropertyEditorId, out id) && id.Equals(propEdIdasGuid);
                    }))
                {
                    var parameters = method.GetParameters();

                    var expressionParams = parameters.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();
                    var expression = Expression.Call(method, expressionParams.Cast<Expression>());

                    var sig = new Signature(!string.IsNullOrWhiteSpace(attrib.Name) ? attrib.Name : method.Name,
                        method.ReturnType, parameters.Select(x => new Parameter(x.Name, x.ParameterType)).Take(1).Concat(
                            parameters.Select(x => new Parameter(x.Name, x.ParameterType)).Skip((parameters[0].ParameterType == typeof(TypedAttribute)) ? 1 : 2)).ToArray()); // Skip the second param if not TypedAttribue, as we'll assume its the property alais
                    var dynamicMethod = new DynamicMethod(sig, Expression.Lambda(expression, expressionParams),
                        (parameters[0].ParameterType == typeof(TypedAttribute)) ? typeof(TypedAttribute) : null,
                        (parameters[0].ParameterType == typeof(TypedAttribute)) ? null : typeof(TParentType));

                    bendyObject.AddMethod(dynamicMethod);
                }
            }
        }
    }
}
