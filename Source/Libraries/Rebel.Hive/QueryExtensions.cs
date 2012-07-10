namespace Rebel.Hive
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Rebel.Framework;
    using Rebel.Framework.Diagnostics;
    using Rebel.Framework.Expressions.Remotion;
    using Rebel.Framework.Persistence.Model.Associations;
    using Rebel.Framework.Persistence.Model.Constants;
    using Rebel.Hive.Linq.Structure;

    public static class QueryExtensions
    {
        /// <summary>
        /// Adds metadata to a query, to filter the <paramref name="source"/> based on the name of a revision type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="revisionType">Type of the revision.</param>
        /// <returns></returns>
        public static IQueryable<T> OfRevisionType<T>(this IQueryable<T> source, string revisionType) where T : class
        {
            return OfRevisionType(source, new RevisionStatusType(revisionType, revisionType));
        }

        /// <summary>
        /// Adds metadata to a query, to filter the <paramref name="source"/> based on the name of a revision type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="revisionType">Type of the revision.</param>
        /// <returns></returns>
        public static IQueryable<T> OfRevisionType<T>(this IQueryable<T> source, RevisionStatusType revisionType) where T : class
        {
            LogHelper.TraceIfEnabled(typeof(QueryExtensions), "In OfRevisionType");

            var currentGenericMethod = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T));
            ExpressionNodeModifierRegistry.Current.EnsureRegistered(
                currentGenericMethod.GetGenericMethodDefinition(), typeof(RevisionFilterExpressionNode));

            Expression<Func<IQueryable<T>>> expr = () => OfRevisionType(source, revisionType);
            return source.Provider.CreateQuery<T>(expr.Body);
        }

        public static IQueryable<T> InIds<T>(this IQueryable<T> source, IEnumerable<HiveId> ids) where T : class
        {
            return InIds(source, ids.ToArray());
        }

        public static IQueryable<T> InIds<T>(this IQueryable<T> source, params HiveId[] ids) where T : class
        {
            LogHelper.TraceIfEnabled(typeof(QueryExtensions), "In InIds");

            var currentGenericMethod = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T));
            ExpressionNodeModifierRegistry.Current.EnsureRegistered(currentGenericMethod.GetGenericMethodDefinition(), typeof(IdFilterExpressionNode));

            // If no ids were specified, the fact this method was called at all dictates we must filter the results to nothing
            if (ids.Any())
            {
                Expression<Func<IQueryable<T>>> expr = () => InIds(source, ids);
                return source.Provider.CreateQuery<T>(expr.Body);
            }
            else
                return Enumerable.Empty<T>().AsQueryable();
        }

        public static IQueryable<T> Paged<T>(this IQueryable<T> source, int pageNumber, int pageSize)
            where T : class
        {
            var currentGenericMethod = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T));
            ExpressionNodeModifierRegistry.Current.EnsureRegistered(
                currentGenericMethod.GetGenericMethodDefinition(), typeof(PaginationFilterExpressionNode));

            Expression<Func<IQueryable<T>>> expr = () => Paged(source, pageNumber, pageSize);
            return source.Provider.CreateQuery<T>(expr.Body);
        }

        public static IQueryable<T> WithParentIds<T>(this IQueryable<T> source, IEnumerable<HiveId> parentIds)
            where T : class
        {
            return WithParentIds(source, FixedRelationTypes.DefaultRelationType, parentIds.ToArray());
        }

        public static IQueryable<T> WithParentIds<T>(this IQueryable<T> source, HiveId parentId, params HiveId[] parentIds)
            where T : class
        {
            return WithParentIds(source, FixedRelationTypes.DefaultRelationType, parentId.AsEnumerableOfOne().Concat(parentIds).ToArray());
        }

        public static IQueryable<T> WithParentIds<T>(this IQueryable<T> source, RelationType relationType, params HiveId[] parentIds)
            where T : class
        {
            var currentGenericMethod = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T));
            ExpressionNodeModifierRegistry.Current.EnsureRegistered(
                currentGenericMethod.GetGenericMethodDefinition(), typeof(ParentIdFilterExpressionNode));

            Expression<Func<IQueryable<T>>> expr = () => WithParentIds(source, relationType, parentIds);
            return source.Provider.CreateQuery<T>(expr.Body);
        }


        public static IQueryable<T> ExcludeParentIds<T>(this IQueryable<T> source, IEnumerable<HiveId> parentIds)
            where T : class
        {
            return ExcludeParentIds(source, FixedRelationTypes.DefaultRelationType, parentIds.ToArray());
        }

        public static IQueryable<T> ExcludeParentIds<T>(this IQueryable<T> source, HiveId parentId, params HiveId[] parentIds)
            where T : class
        {
            return ExcludeParentIds(source, FixedRelationTypes.DefaultRelationType, parentId.AsEnumerableOfOne().Concat(parentIds).ToArray());
        }

        public static IQueryable<T> ExcludeParentIds<T>(this IQueryable<T> source, RelationType relationType, params HiveId[] parentIds)
            where T : class
        {
            var currentGenericMethod = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T));
            ExpressionNodeModifierRegistry.Current.EnsureRegistered(
                currentGenericMethod.GetGenericMethodDefinition(), typeof(ExcludeParentIdFilterExpressionNode));

            Expression<Func<IQueryable<T>>> expr = () => ExcludeParentIds(source, relationType, parentIds);
            return source.Provider.CreateQuery<T>(expr.Body);
        }

        public static IQueryable<T> ExcludeIds<T>(this IQueryable<T> source, IEnumerable<HiveId> excludeIds)
            where T : class
        {
            return ExcludeIds(source, excludeIds.ToArray());
        }

        public static IQueryable<T> ExcludeIds<T>(this IQueryable<T> source, HiveId excludeId, params HiveId[] excludeIds)
            where T : class
        {
            return ExcludeIds(source, excludeId.AsEnumerableOfOne().Concat(excludeIds).ToArray());
        }

        public static IQueryable<T> ExcludeIds<T>(this IQueryable<T> source, params HiveId[] excludeIds)
            where T : class
        {
            var currentGenericMethod = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(T));
            ExpressionNodeModifierRegistry.Current.EnsureRegistered(
                currentGenericMethod.GetGenericMethodDefinition(), typeof(ExcludeIdFilterExpressionNode));

            Expression<Func<IQueryable<T>>> expr = () => ExcludeIds(source, excludeIds);
            return source.Provider.CreateQuery<T>(expr.Body);
        }
    }
}