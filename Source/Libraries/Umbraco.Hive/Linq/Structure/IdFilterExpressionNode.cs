namespace Umbraco.Hive.Linq.Structure
{
    using System.Linq.Expressions;
    using Remotion.Linq.Clauses;
    using Remotion.Linq.Parsing.Structure.IntermediateModel;

    public class IdFilterExpressionNode : ResultOperatorExpressionNodeBase
    {
        public Expression IdList { get; protected set; }

        public IdFilterExpressionNode(MethodCallExpressionParseInfo parseInfo, Expression idList)
            : base(parseInfo, (LambdaExpression)null, (LambdaExpression)null)
        {
            IdList = idList;
        }

        public override Expression Resolve(ParameterExpression inputParameter, Expression expressionToBeResolved, ClauseGenerationContext clauseGenerationContext)
        {
            return Source.Resolve(inputParameter, expressionToBeResolved, clauseGenerationContext);
        }

        protected override ResultOperatorBase CreateResultOperator(ClauseGenerationContext clauseGenerationContext)
        {
            return new IdFilterResultOperator(IdList);
        }
    }

    public class ExcludeIdFilterExpressionNode : ResultOperatorExpressionNodeBase
    {
        public Expression ExclusionIdList { get; protected set; }

        public ExcludeIdFilterExpressionNode(MethodCallExpressionParseInfo parseInfo, Expression idList)
            : base(parseInfo, (LambdaExpression)null, (LambdaExpression)null)
        {
            ExclusionIdList = idList;
        }

        public override Expression Resolve(ParameterExpression inputParameter, Expression expressionToBeResolved, ClauseGenerationContext clauseGenerationContext)
        {
            return Source.Resolve(inputParameter, expressionToBeResolved, clauseGenerationContext);
        }

        protected override ResultOperatorBase CreateResultOperator(ClauseGenerationContext clauseGenerationContext)
        {
            return new ExcludeIdFilterResultOperator(ExclusionIdList);
        }
    }
}