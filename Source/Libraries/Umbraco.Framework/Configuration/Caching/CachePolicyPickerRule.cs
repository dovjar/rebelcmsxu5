namespace Umbraco.Framework.Configuration.Caching
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration;
    using System.Linq;
    using System.Threading;
    using Umbraco.Framework.Diagnostics;

    public class CachePolicyPickerRule : ConfigurationElement
    {
        [ConfigurationProperty("forKeyType", IsRequired = true)]
        public string ForKeyType
        {
            get { return (string)this["forKeyType"]; }
            set { base["forKeyType"] = value; }
        }

        [ConfigurationProperty("check", IsRequired = true)]
        public string Expression
        {
            get { return (string)this["check"]; }
            set { base["check"] = value; }
        }

        [ConfigurationProperty("usePolicy", IsRequired = false)]
        public string UsePolicyName
        {
            get { return (string)this["usePolicy"]; }
            set { base["usePolicy"] = value; }
        }

        private bool _couldNotParseKeyType = false;
        private bool _couldNotParseParameterValue = false;
        private List<Type> _couldNotParseExpressionAgainstTypes = new List<Type>();
        private Delegate _dynamicDelegate;
        private ReaderWriterLockSlim _delegateCompilerLocker = new ReaderWriterLockSlim();
        private List<object> _parameterValueCache = new List<object>();

        public bool KeyMatches(object providedKeyInstance)
        {
            // Check if this rule has been turned off by a previous parse attempt
            if (_couldNotParseKeyType || _couldNotParseParameterValue || providedKeyInstance == null) return false;

            var providedKeyType = providedKeyInstance.GetType();
            var keyType = providedKeyType;
            if (_couldNotParseExpressionAgainstTypes.Contains(keyType)) return false;

            var declaredKeyType = Type.GetType(ForKeyType, false, true);
            if (declaredKeyType == null)
            {
                _couldNotParseKeyType = true;
                return false;
            }

            // Exit quickly if we know that the provided key isn't assignable to the key type for this rule
            if (!TypeFinder.IsTypeAssignableFrom(declaredKeyType, providedKeyType))
                return false;

            // We only need to parse the values of the parameters once for the life of the application
            // since it's based from configuration
            if (!_parameterValueCache.Any())
            {
                using (new WriteLockDisposable(_delegateCompilerLocker))
                {
                    // Lock double-check
                    if (!_parameterValueCache.Any())
                    {
                        foreach (var parameter in Params.Cast<CachePolicyPickerRuleParameter>())
                        {
                            var paramType = Type.GetType(parameter.Type, false, true);
                            if (paramType == null) return false;

                            object value = null;

                            TypeConverter converter = TypeDescriptor.GetConverter(typeof(string));
                            if (converter.CanConvertTo(paramType))
                            {
                                value = converter.ConvertTo(parameter.ValueAsString, paramType);
                            }

                            // Don't bother converting the other way if we already got a value
                            if (value == null)
                            {
                                converter = TypeDescriptor.GetConverter(paramType);
                                if (converter.CanConvertFrom(typeof(string)))
                                {
                                    value = converter.ConvertFrom(parameter.ValueAsString);
                                }
                            }

                            // If we couldn't parse one of the parameters, then we can essentially
                            // turn off this entire rule
                            if (value == null)
                            {
                                _couldNotParseParameterValue = true;
                                return false;
                            }

                            _parameterValueCache.Add(value);
                        }
                    }
                }
            }

            try
            {
                if (_dynamicDelegate == null)
                {
                    using (new WriteLockDisposable(_delegateCompilerLocker))
                    {
                        if (_dynamicDelegate == null)
                        {
                            var lambda = System.Linq.Dynamic.DynamicExpression.ParseLambda(providedKeyType, typeof(bool), Expression, _parameterValueCache.ToArray());
                            _dynamicDelegate = lambda.Compile();
                        }
                    }
                }
                if (_dynamicDelegate == null)
                {
                    _couldNotParseExpressionAgainstTypes.Add(providedKeyType);
                    return false;
                }
                var result = _dynamicDelegate.DynamicInvoke(new[] { providedKeyInstance }) as bool?;
                if (result.HasValue) return result.Value;
            }
            catch (Exception ex)
            {
                LogHelper.Warn<General>("Could not parse cache policy rule [{0}], message: {1}".InvariantFormat(Expression, ex.Message));
            }

            return false;
        }

        [ConfigurationProperty("", IsRequired = false, IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(CachePolicyPickerRuleParameter), AddItemName = "param")]
        public CachePolicyPickerRuleParameterCollection Params
        {
            get { return (CachePolicyPickerRuleParameterCollection)this[""]; }
        }
    }
}