namespace Umbraco.Hive.Linq.Structure
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Remotion.Linq.Clauses;
    using Umbraco.Framework;
    using Umbraco.Framework.Expressions.Remotion;
    using Umbraco.Framework.Linq.QueryModel;

    public class IdFilterResultOperator : AbstractExtensionResultOperator
    {
        public IdFilterResultOperator(Expression parameter)
            : base(parameter)
        {
        }

        public override void ModifyQueryDescription(QueryDescriptionBuilder queryDescription, Type resultType)
        {
            var constant = FirstParameter as ConstantExpression;
            queryDescription.From.RequiredEntityIds = (IEnumerable<HiveId>)(constant.Value);
        }

        public override string Name
        {
            get
            {
                return "IdFilter";
            }
        }

        public override ResultOperatorBase Clone(CloneContext cloneContext)
        {
            return new IdFilterResultOperator(FirstParameter);
        }
    }

    public class ExcludeIdFilterResultOperator : AbstractExtensionResultOperator
    {
        public ExcludeIdFilterResultOperator(Expression parameter)
            : base(parameter)
        {
        }

        public override void ModifyQueryDescription(QueryDescriptionBuilder queryDescription, Type resultType)
        {
            var constant = FirstParameter as ConstantExpression;
            queryDescription.From.ExcludeEntityIds = (IEnumerable<HiveId>)(constant.Value);
        }

        public override string Name
        {
            get
            {
                return "IdFilter";
            }
        }

        public override ResultOperatorBase Clone(CloneContext cloneContext)
        {
            return new ExcludeIdFilterResultOperator(FirstParameter);
        }
    }
}