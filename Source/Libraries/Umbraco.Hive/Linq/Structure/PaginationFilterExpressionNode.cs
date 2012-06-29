namespace Umbraco.Hive.Linq.Structure
{
    using System.Linq.Expressions;
    using Remotion.Linq.Clauses;
    using Remotion.Linq.Parsing.Structure.IntermediateModel;

    public class PaginationFilterExpressionNode : ResultOperatorExpressionNodeBase
    {
        public Expression PageNumber { get; protected set; }
        public Expression PageSize { get; protected set; }

        public PaginationFilterExpressionNode(MethodCallExpressionParseInfo parseInfo, Expression pageNumber, Expression pageSize)
            : base(parseInfo, (LambdaExpression)null, (LambdaExpression)null)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        public override Expression Resolve(ParameterExpression inputParameter, Expression expressionToBeResolved, ClauseGenerationContext clauseGenerationContext)
        {
            return Source.Resolve(inputParameter, expressionToBeResolved, clauseGenerationContext);
        }

        protected override ResultOperatorBase CreateResultOperator(ClauseGenerationContext clauseGenerationContext)
        {
            return new PaginationFilterResultOperator(PageNumber, PageSize);
        }
    }

    public class ParentIdFilterExpressionNode : ResultOperatorExpressionNodeBase
    {
        public Expression RelationType { get; protected set; }
        public Expression ParentIds { get; protected set; }

        public ParentIdFilterExpressionNode(MethodCallExpressionParseInfo parseInfo, Expression relationType, Expression parentIds)
            : base(parseInfo, (LambdaExpression)null, (LambdaExpression)null)
        {
            RelationType = relationType;
            ParentIds = parentIds;
        }

        public override Expression Resolve(ParameterExpression inputParameter, Expression expressionToBeResolved, ClauseGenerationContext clauseGenerationContext)
        {
            return Source.Resolve(inputParameter, expressionToBeResolved, clauseGenerationContext);
        }

        protected override ResultOperatorBase CreateResultOperator(ClauseGenerationContext clauseGenerationContext)
        {
            return new ParentIdFilterResultOperator(RelationType, ParentIds);
        }
    }

    public class ExcludeParentIdFilterExpressionNode : ResultOperatorExpressionNodeBase
    {
        public Expression RelationType { get; protected set; }
        public Expression ExcludeParentIds { get; protected set; }

        public ExcludeParentIdFilterExpressionNode(MethodCallExpressionParseInfo parseInfo, Expression relationType, Expression parentIds)
            : base(parseInfo, (LambdaExpression)null, (LambdaExpression)null)
        {
            RelationType = relationType;
            ExcludeParentIds = parentIds;
        }

        public override Expression Resolve(ParameterExpression inputParameter, Expression expressionToBeResolved, ClauseGenerationContext clauseGenerationContext)
        {
            return Source.Resolve(inputParameter, expressionToBeResolved, clauseGenerationContext);
        }

        protected override ResultOperatorBase CreateResultOperator(ClauseGenerationContext clauseGenerationContext)
        {
            return new ExcludeParentIdFilterResultOperator(RelationType, ExcludeParentIds);
        }
    }
}