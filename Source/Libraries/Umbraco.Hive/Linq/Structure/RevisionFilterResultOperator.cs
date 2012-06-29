namespace Umbraco.Hive.Linq.Structure
{
    using System;
    using System.Linq.Expressions;
    using Remotion.Linq.Clauses;
    using Umbraco.Framework;
    using Umbraco.Framework.Expressions.Remotion;
    using Umbraco.Framework.Linq.QueryModel;

    public class RevisionFilterResultOperator : AbstractExtensionResultOperator
    {
        public RevisionFilterResultOperator(Expression parameter)
            : base(parameter)
        {
        }

        #region Overrides of AbstractExtensionResultOperator

        public override void ModifyQueryDescription(QueryDescriptionBuilder queryDescription, Type resultType)
        {
            var constant = FirstParameter as ConstantExpression;
            queryDescription.From.RevisionStatusType = (RevisionStatusType)(constant.Value);
        }

        public override string Name
        {
            get
            {
                return "RevisionFilter";
            }
        }

        public override ResultOperatorBase Clone(CloneContext cloneContext)
        {
            return new RevisionFilterResultOperator(FirstParameter);
        }

        #endregion
    }
}