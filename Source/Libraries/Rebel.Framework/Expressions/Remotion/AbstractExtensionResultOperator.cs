namespace Rebel.Framework.Expressions.Remotion
{
    using System;
    using System.Linq.Expressions;
    using Rebel.Framework.Linq.QueryModel;
    using global::Remotion.Linq.Clauses;
    using global::Remotion.Linq.Clauses.ExpressionTreeVisitors;
    using global::Remotion.Linq.Clauses.ResultOperators;
    using global::Remotion.Linq.Clauses.StreamedData;

    public abstract class AbstractExtensionResultOperator : SequenceTypePreservingResultOperatorBase
    {
        protected AbstractExtensionResultOperator(Expression parameter)
        {
            FirstParameter = parameter;
        }

        public abstract void ModifyQueryDescription(QueryDescriptionBuilder queryDescription, Type resultType);

        public Expression FirstParameter { get; private set; }

        public abstract string Name { get; }

        public override string ToString()
        {
            return Name + " (" + FormattingExpressionTreeVisitor.Format(FirstParameter) + ")";
        }

        public override abstract ResultOperatorBase Clone(CloneContext cloneContext);

        public override void TransformExpressions(Func<Expression, Expression> transformation)
        {
            FirstParameter = transformation(FirstParameter);
        }

        public override StreamedSequence ExecuteInMemory<T>(StreamedSequence input)
        {
            return input; // sequence is not changed by this operator
        }
    }
}