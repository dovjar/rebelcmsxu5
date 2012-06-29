namespace Umbraco.Hive.Linq.Structure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Remotion.Linq.Clauses;
    using Umbraco.Framework;
    using Umbraco.Framework.Data;
    using Umbraco.Framework.Expressions.Remotion;
    using Umbraco.Framework.Linq.QueryModel;
    using Umbraco.Framework.Persistence.Model.Associations;
    using Umbraco.Framework.Persistence.Model.Constants;

    public class ParentIdFilterResultOperator : AbstractExtensionResultOperator
    {
        public ParentIdFilterResultOperator(Expression relationType, Expression parentIds)
            : base(relationType)
        {
            ParentIds = parentIds;
        }

        public Expression ParentIds { get; private set; }

        public override void ModifyQueryDescription(QueryDescriptionBuilder queryDescription, Type resultType)
        {
            var relationTypeExpr = FirstParameter as ConstantExpression;
            var parentIdsExpr = ParentIds as ConstantExpression;

            if (relationTypeExpr == null || parentIdsExpr == null)
                return;

            var relationType = (relationTypeExpr.Value as RelationType) ?? FixedRelationTypes.DefaultRelationType;
            var parentIds = (parentIdsExpr.Value as IEnumerable<HiveId>) ?? Enumerable.Empty<HiveId>();

            if (parentIds.Any())
            {
                queryDescription.From.HierarchyScope = HierarchyScope.Children;
                queryDescription.From.ScopeStartIds = parentIds;
                queryDescription.From.HierarchyType = relationType.RelationName;
            }
        }

        public override string Name
        {
            get
            {
                return "ParentIdFilter";
            }
        }

        public override void TransformExpressions(System.Func<Expression, Expression> transformation)
        {
            base.TransformExpressions(transformation);
            transformation.Invoke(ParentIds);
        }

        public override ResultOperatorBase Clone(CloneContext cloneContext)
        {
            return new ParentIdFilterResultOperator(FirstParameter, ParentIds);
        }
    }

    public class ExcludeParentIdFilterResultOperator : AbstractExtensionResultOperator
    {
        public ExcludeParentIdFilterResultOperator(Expression relationType, Expression parentIds)
            : base(relationType)
        {
            ExcludeParentIds = parentIds;
        }

        public Expression ExcludeParentIds { get; private set; }

        public override void ModifyQueryDescription(QueryDescriptionBuilder queryDescription, Type resultType)
        {
            var relationTypeExpr = FirstParameter as ConstantExpression;
            var parentIdsExpr = ExcludeParentIds as ConstantExpression;

            if (relationTypeExpr == null || parentIdsExpr == null)
                return;

            var relationType = (relationTypeExpr.Value as RelationType) ?? FixedRelationTypes.DefaultRelationType;
            var parentIds = (parentIdsExpr.Value as IEnumerable<HiveId>) ?? Enumerable.Empty<HiveId>();

            if (parentIds.Any())
            {
                queryDescription.From.ExcludeParentIds = parentIds;
            }
        }

        public override string Name
        {
            get
            {
                return "ExcludeParentIdFilter";
            }
        }

        public override void TransformExpressions(System.Func<Expression, Expression> transformation)
        {
            base.TransformExpressions(transformation);
            transformation.Invoke(ExcludeParentIds);
        }

        public override ResultOperatorBase Clone(CloneContext cloneContext)
        {
            return new ExcludeParentIdFilterResultOperator(FirstParameter, ExcludeParentIds);
        }
    }

    public class PaginationFilterResultOperator : AbstractExtensionResultOperator
    {
        public PaginationFilterResultOperator(Expression pageNumber, Expression pageSize)
            : base(pageNumber)
        {
            PageSize = pageSize;
        }

        public Expression PageSize { get; private set; }

        public override void ModifyQueryDescription(QueryDescriptionBuilder queryDescription, Type resultType)
        {
            var pageNumberExpr = FirstParameter as ConstantExpression;
            var pageSizeExpr = PageSize as ConstantExpression;

            if (pageNumberExpr == null || pageSizeExpr == null)
                return;

            int? pageNumber = pageNumberExpr.Value as int?;
            int? pageSize = pageSizeExpr.Value as int?;

            if (pageNumber.HasValue && pageSize.HasValue)
            {
                queryDescription.AddSkipResultFilter(resultType, pageSize.Value * (pageNumber.Value - 1));
                queryDescription.AddTakeResultFilter(resultType, pageSize.Value);
            }
        }

        public override string Name
        {
            get
            {
                return "PaginationFilter";
            }
        }

        public override void TransformExpressions(System.Func<Expression, Expression> transformation)
        {
            base.TransformExpressions(transformation);
            transformation.Invoke(PageSize);
        }

        public override ResultOperatorBase Clone(CloneContext cloneContext)
        {
            return new PaginationFilterResultOperator(FirstParameter, PageSize);
        }
    }
}