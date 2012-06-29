using System.Linq.Expressions;
using Umbraco.Framework;
using Umbraco.Framework.Dynamics.Expressions;
using Umbraco.Framework.Persistence.ModelFirst;

namespace Umbraco.Cms.Web.LinqSupport
{
    using Umbraco.Framework.Linq.CriteriaGeneration.Expressions;

    using Umbraco.Framework.Linq.CriteriaGeneration.StructureMetadata;
    using Umbraco.Framework.Persistence.Model.LinqSupport;

    /// <summary>
    /// Provides a mechanism to assess certain types of expression and determine whether they are supported by the expression binding provider. 
    /// </summary>
    /// <remarks></remarks>
    public class ViewModelStructureBinder : PersistenceModelStructureBinder
    {
        /// <summary>
        /// Creates a <see cref="FieldSelectorExpression"/> from a <see cref="MethodCallExpression"/>.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="reportedSignatureSupport">A component outlining the supported expression structure of this provider.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public override FieldSelectorExpression CreateFieldSelector(MethodCallExpression expression, BindingSignatureSupport reportedSignatureSupport)
        {
            switch (expression.Method.Name)
            {
                case "Field":
                    // This is an extension method for the RenderViewModel so the first argument is the extended object
                    return new FieldSelectorExpression(((ConstantExpression)expression.Arguments[1]).Value.ToString());
                case "StringField":
                    // This is an extension method for the RenderViewModel so the first argument is the extended object
                    return new FieldSelectorExpression(((ConstantExpression)expression.Arguments[1]).Value.ToString());
                case "NumberField":
                    // This is an extension method for the RenderViewModel so the first argument is the extended object
                    return new FieldSelectorExpression(((ConstantExpression)expression.Arguments[1]).Value.ToString());
                case "BooleanField":
                    // This is an extension method for the RenderViewModel so the first argument is the extended object
                    return new FieldSelectorExpression(((ConstantExpression)expression.Arguments[1]).Value.ToString());
                case "get_Item":
                    // This is the default accessor of a Dictionary, so check if the parent object is supported too
                    if (ExpressionHelper.IsMember(expression.Object) && IsSupportedMember(expression.Object as MemberExpression).SignatureSupportType != SignatureSupportType.NotSupported)
                        return new FieldSelectorExpression(((ConstantExpression) expression.Arguments[0]).Value.ToString());
                    break;
            }

            return base.CreateFieldSelector(expression, reportedSignatureSupport);
        }

        /// <summary>
        /// Creates a field selector.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="innerFieldName">Name of the inner field.</param>
        /// <param name="reportedSignatureSupport">The reported signature support.</param>
        /// <returns></returns>
        public override FieldSelectorExpression CreateFieldSelector(string fieldName, string innerFieldName, BindingSignatureSupport reportedSignatureSupport)
        {
            switch (fieldName)
            {
                case "Name":
                    return new FieldSelectorExpression("system-internal-node-name", fieldName);
                case "UrlName":
                    return new FieldSelectorExpression("system-internal-node-name", fieldName);
                case "Template":
                case "TemplateId":
                case "CurrentTemplate":
                    return new FieldSelectorExpression("system-internal-selected-template", "TemplateId");
                default:
                    return base.CreateFieldSelector(fieldName, innerFieldName, reportedSignatureSupport);
            }
        }

        /// <summary>
        /// Determines whether the expression represents a supported member access call.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <remarks></remarks>
        public override BindingSignatureSupport IsSupportedMember(MemberExpression expression)
        {
            // Account for if this is a schema access
            var memberParent = expression.Expression as MemberExpression;
            if (memberParent != null)
            {
                switch (memberParent.Member.Name)
                {
                    case "ContentType":
                        switch (expression.Member.Name)
                        {
                            case "Alias":
                                return new BindingSignatureSupport(SignatureSupportType.SupportedAsSchemaAlias, ValuePredicateType.Equal);
                        }
                        return new BindingSignatureSupport(SignatureSupportType.SupportedAsSchemaMetaDataValue, ValuePredicateType.Equal);
                    case "CurrentTemplate":
                        switch (expression.Member.Name)
                        {
                            case "Alias":
                            case "Id":
                                return new BindingSignatureSupport(SignatureSupportType.SupportedAsFieldName, ValuePredicateType.StartsWith);
                        }
                        return new BindingSignatureSupport(SignatureSupportType.SupportedAsSchemaMetaDataValue, ValuePredicateType.Equal);
                }
            }

            // If it's not schema, treat it like it's a field expression
            switch (expression.Member.Name)
            {
                case "Fields":
                    return new BindingSignatureSupport(SignatureSupportType.SupportedAsFieldName, ValuePredicateType.Equal);
            }

            return base.IsSupportedMember(expression);
        }

        public override BindingSignatureSupport IsSupportedMethod(MethodCallExpression expression)
        {
            // Add support for dynamic expressions using an intermediary set of "fake" methods on DynamicMemberMetadata
            if (expression.Method == DynamicMemberMetadata.GetMemberMethod)
            {
                var firstArg = expression.Arguments[0] as ConstantExpression;
                if (firstArg != null)
                    switch (firstArg.Value as string)
                    {
                        case "Template":
                        case "TemplateId":
                            return new BindingSignatureSupport(SignatureSupportType.SupportedAsFieldName, ValuePredicateType.StartsWith);
                    }
            }

            return base.IsSupportedMethod(expression);
        }
    }
}