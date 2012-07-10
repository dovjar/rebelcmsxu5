using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Rebel.Cms.Web.Model;
using Rebel.Framework;
using Rebel.Framework.Dynamics;
using Rebel.Framework.Dynamics.Attributes;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Hive;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Cms.Web
{
    [DynamicExtensions] // Used to make flagged extensions available to bendy object
    public static class ContentIsHelperExtensions
    {
        #region IsFirst

        /// <summary>
        /// Determines whether the specified content is first.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>
        ///   <c>true</c> if the specified content is first; otherwise, <c>false</c>.
        /// </returns>
        [DynamicExtension]
        public static bool IsFirst(this Content content)
        {
            var hive = DependencyResolver.Current.GetService<IHiveManager>();
            return hive.FrameworkContext.ScopedCache.GetOrCreateTyped<bool>("cihe_IsFirst_" + content.Id, () => 
            {
                using (var uow = hive.OpenReader<IContentStore>())
                {
                    var siblings = uow.Repositories.GetBranchRelations(content.Id, FixedRelationTypes.DefaultRelationType);
                    return siblings.First().DestinationId == content.Id;
                }
            });
        }

        /// <summary>
        /// Determines whether the specified content is first.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsFirst(this Content content, string valueIfTrue)
        {
            return content.IsFirst(valueIfTrue, String.Empty);
        }

        /// <summary>
        /// Determines whether the specified content is first.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <param name="valueIfFalse">The value if false.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsFirst(this Content content, string valueIfTrue, string valueIfFalse)
        {
            return content.IsFirst()
                       ? new HtmlString(valueIfTrue)
                       : new HtmlString(valueIfFalse);
        }

        #endregion

        #region IsNotFirst

        /// <summary>
        /// Determines whether the specified content is not first.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>
        ///   <c>true</c> if the specified content is not first; otherwise, <c>false</c>.
        /// </returns>
        [DynamicExtension]
        public static bool IsNotFirst(this Content content)
        {
            return !content.IsFirst();
        }

        /// <summary>
        /// Determines whether the specified content is not first.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsNotFirst(this Content content, string valueIfTrue)
        {
            return content.IsNotFirst(valueIfTrue, String.Empty);
        }

        /// <summary>
        /// Determines whether the specified content is not first.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <param name="valueIfFalse">The value if false.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsNotFirst(this Content content, string valueIfTrue, string valueIfFalse)
        {
            return content.IsNotFirst()
                       ? new HtmlString(valueIfTrue)
                       : new HtmlString(valueIfFalse);
        }

        #endregion

        #region IsLast

        /// <summary>
        /// Determines whether the specified content is last.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>
        ///   <c>true</c> if the specified content is last; otherwise, <c>false</c>.
        /// </returns>
        [DynamicExtension]
        public static bool IsLast(this Content content)
        {
            var hive = DependencyResolver.Current.GetService<IHiveManager>();
            return hive.FrameworkContext.ScopedCache.GetOrCreateTyped<bool>("cihe_IsLast_" + content.Id, () =>
            {
                using (var uow = hive.OpenReader<IContentStore>())
                {
                    var siblings = uow.Repositories.GetBranchRelations(content.Id, FixedRelationTypes.DefaultRelationType);
                    return siblings.Last().DestinationId == content.Id;
                }
            });
        }

        /// <summary>
        /// Determines whether the specified content is last.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsLast(this Content content, string valueIfTrue)
        {
            return content.IsLast(valueIfTrue, String.Empty);
        }

        /// <summary>
        /// Determines whether the specified content is last.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <param name="valueIfFalse">The value if false.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsLast(this Content content, string valueIfTrue, string valueIfFalse)
        {
            return content.IsLast()
                       ? new HtmlString(valueIfTrue)
                       : new HtmlString(valueIfFalse);
        }

        #endregion

        #region IsNotLast

        /// <summary>
        /// Determines whether the specified content is not last.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>
        ///   <c>true</c> if the specified content is not last; otherwise, <c>false</c>.
        /// </returns>
        [DynamicExtension]
        public static bool IsNotLast(this Content content)
        {
            return !content.IsLast();
        }

        /// <summary>
        /// Determines whether the specified content is not last.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsNotLast(this Content content, string valueIfTrue)
        {
            return content.IsNotLast(valueIfTrue, String.Empty);
        }

        /// <summary>
        /// Determines whether the specified content is not last.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <param name="valueIfFalse">The value if false.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsNotLast(this Content content, string valueIfTrue, string valueIfFalse)
        {
            return content.IsNotLast()
                       ? new HtmlString(valueIfTrue)
                       : new HtmlString(valueIfFalse);
        }

        #endregion

        #region IsIndex

        /// <summary>
        /// Determines whether the specified content is at the specified index.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="index">The index.</param>
        /// <returns>
        ///   <c>true</c> if the specified content is at the specified index; otherwise, <c>false</c>.
        /// </returns>
        [DynamicExtension]
        public static bool IsIndex(this Content content, int index)
        {
            var hive = DependencyResolver.Current.GetService<IHiveManager>();
            return hive.FrameworkContext.ScopedCache.GetOrCreateTyped<bool>("cihe_IsIndex_" + content.Id + "_" + index, () =>
            {
                using (var uow = hive.OpenReader<IContentStore>())
                {
                    var siblings = uow.Repositories.GetBranchRelations(content.Id, FixedRelationTypes.DefaultRelationType);
                    return index >= 0 && index <= siblings.Count() && siblings.ElementAt(index).DestinationId == content.Id;
                }
            });
        }

        /// <summary>
        /// Determines whether the specified content is at the specified index.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="index">The index.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsIndex(this Content content, int index, string valueIfTrue)
        {
            return content.IsIndex(index, valueIfTrue, String.Empty);
        }

        /// <summary>
        /// Determines whether the specified content is at the specified index.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="index">The index.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <param name="valueIfFalse">The value if false.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsIndex(this Content content, int index, string valueIfTrue, string valueIfFalse)
        {
            return content.IsIndex(index)
                       ? new HtmlString(valueIfTrue)
                       : new HtmlString(valueIfFalse);
        }

        #endregion

        #region IsNotIndex

        /// <summary>
        /// Determines whether the specified content is not at the specified index.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="index">The index.</param>
        /// <returns>
        ///   <c>true</c> if the specified content is not at the specified index; otherwise, <c>false</c>.
        /// </returns>
        [DynamicExtension]
        public static bool IsNotIndex(this Content content, int index)
        {
            return !content.IsIndex(index);
        }

        /// <summary>
        /// Determines whether the specified content is not at the specified index.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="index">The index.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsNotIndex(this Content content, int index, string valueIfTrue)
        {
            return content.IsNotIndex(index, valueIfTrue, String.Empty);
        }

        /// <summary>
        /// Determines whether the specified content is not at the specified index.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="index">The index.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <param name="valueIfFalse">The value if false.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsNotIndex(this Content content, int index, string valueIfTrue, string valueIfFalse)
        {
            return content.IsNotIndex(index)
                       ? new HtmlString(valueIfTrue)
                       : new HtmlString(valueIfFalse);
        }

        #endregion

        #region IsPosition

        /// <summary>
        /// Determines whether the specified content is at the specified position.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="pos">The position.</param>
        /// <returns>
        ///   <c>true</c> if the specified content is at the specified position; otherwise, <c>false</c>.
        /// </returns>
        [DynamicExtension]
        public static bool IsPosition(this Content content, int pos)
        {
            return content.IsIndex(pos - 1);
        }

        /// <summary>
        /// Determines whether the specified content is specified position.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="pos">The position.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsPosition(this Content content, int pos, string valueIfTrue)
        {
            return content.IsPosition(pos, valueIfTrue, String.Empty);
        }

        /// <summary>
        /// Determines whether the specified content is specified position.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="pos">The position.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <param name="valueIfFalse">The value if false.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsPosition(this Content content, int pos, string valueIfTrue, string valueIfFalse)
        {
            return content.IsPosition(pos)
                       ? new HtmlString(valueIfTrue)
                       : new HtmlString(valueIfFalse);
        }

        #endregion

        #region IsNotPosition

        /// <summary>
        /// Determines whether the specified content is not at the specified index position.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="index">The index.</param>
        /// <returns>
        ///   <c>true</c> if the specified content is not at the specified index position; otherwise, <c>false</c>.
        /// </returns>
        [DynamicExtension]
        public static bool IsNotPosition(this Content content, int index)
        {
            return !content.IsPosition(index);
        }

        /// <summary>
        /// Determines whether the specified content is not at the specified index position.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="index">The index.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsNotPosition(this Content content, int index, string valueIfTrue)
        {
            return content.IsNotPosition(index, valueIfTrue, String.Empty);
        }

        /// <summary>
        /// Determines whether the specified content is not at the specified index position.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="index">The index.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <param name="valueIfFalse">The value if false.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsNotPosition(this Content content, int index, string valueIfTrue, string valueIfFalse)
        {
            return content.IsNotPosition(index)
                       ? new HtmlString(valueIfTrue)
                       : new HtmlString(valueIfFalse);
        }

        #endregion

        #region IsPositionDivisibleBy()

        /// <summary>
        /// Determines whether the specified content's position is evenly divisible by the specified value.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the specified contents position is evenly divisible by the specified mod value; otherwise, <c>false</c>.
        /// </returns>
        [DynamicExtension]
        public static bool IsPositionDivisibleBy(this Content content, int value)
        {
            var hive = DependencyResolver.Current.GetService<IHiveManager>();
            return hive.FrameworkContext.ScopedCache.GetOrCreateTyped<bool>("cihe_IsPositionDivisibleBy_" + content.Id + "_" + value, () =>
            {
                using (var uow = hive.OpenReader<IContentStore>())
                {
                    var siblings = uow.Repositories.GetBranchRelations(content.Id, FixedRelationTypes.DefaultRelationType);
                    var currentIndex = siblings.FindIndex(sibling => sibling.DestinationId == content.Id);
                    return currentIndex >= 0 && (currentIndex + 1) % value == 0;
                }
            });
        }

        /// <summary>
        /// Determines whether the specified content's position is evenly divisible by the specified value.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="value">The value.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsPositionDivisibleBy(this Content content, int value, string valueIfTrue)
        {
            return content.IsPositionDivisibleBy(value, valueIfTrue, String.Empty);
        }

        /// <summary>
        /// Determines whether the specified content's position is evenly divisible by the specified value.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="value">The value.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <param name="valueIfFalse">The value if false.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsPositionDivisibleBy(this Content content, int value, string valueIfTrue, string valueIfFalse)
        {
            return content.IsPositionDivisibleBy(value)
                       ? new HtmlString(valueIfTrue)
                       : new HtmlString(valueIfFalse);
        }

        #endregion

        #region IsPositionIndivisibleBy

        /// <summary>
        /// Determines whether the specified content's position is not evenly divisible by the specified mod value.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="mod">The mod.</param>
        /// <returns>
        ///   <c>true</c> if the specified contents position is not evenly divisible by the specified mod value; otherwise, <c>false</c>.
        /// </returns>
        [DynamicExtension]
        public static bool IsPositionIndivisibleBy(this Content content, int mod)
        {
            return !content.IsPositionDivisibleBy(mod);
        }

        /// <summary>
        /// Determines whether the specified content's position is not evenly divisible by the specified mod value.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="mod">The mod.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsPositionIndivisibleBy(this Content content, int mod, string valueIfTrue)
        {
            return content.IsPositionIndivisibleBy(mod, valueIfTrue, String.Empty);
        }

        /// <summary>
        /// Determines whether the specified content's position is not evenly divisible by the specified mod value.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="mod">The mod.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <param name="valueIfFalse">The value if false.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsPositionIndivisibleBy(this Content content, int mod, string valueIfTrue, string valueIfFalse)
        {
            return content.IsPositionIndivisibleBy(mod)
                       ? new HtmlString(valueIfTrue)
                       : new HtmlString(valueIfFalse);
        }

        #endregion

        #region IsEven

        /// <summary>
        /// Determines whether the specified content is at an even index position.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>
        ///   <c>true</c> if the specified content is at an even index position; otherwise, <c>false</c>.
        /// </returns>
        [DynamicExtension]
        public static bool IsEven(this Content content)
        {
            return content.IsPositionDivisibleBy(2);
        }

        /// <summary>
        /// Determines whether the specified content is at an even index position.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsEven(this Content content, string valueIfTrue)
        {
            return content.IsEven(valueIfTrue, String.Empty);
        }

        /// <summary>
        /// Determines whether the specified content is at an even index position.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <param name="valueIfFalse">The value if false.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsEven(this Content content, string valueIfTrue, string valueIfFalse)
        {
            return content.IsEven()
                       ? new HtmlString(valueIfTrue)
                       : new HtmlString(valueIfFalse);
        }

        #endregion

        #region IsOdd

        /// <summary>
        /// Determines whether the specified content is at and odd index position.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>
        ///   <c>true</c> if the specified content is at and odd index position; otherwise, <c>false</c>.
        /// </returns>
        [DynamicExtension]
        public static bool IsOdd(this Content content)
        {
            return !content.IsEven();
        }

        /// <summary>
        /// Determines whether the specified content is at and odd index position.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsOdd(this Content content, string valueIfTrue)
        {
            return content.IsOdd(valueIfTrue, String.Empty);
        }

        /// <summary>
        /// Determines whether the specified content is at and odd index position.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <param name="valueIfFalse">The value if false.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsOdd(this Content content, string valueIfTrue, string valueIfFalse)
        {
            return content.IsOdd()
                       ? new HtmlString(valueIfTrue)
                       : new HtmlString(valueIfFalse);
        }

        #endregion

        #region IsEqualTo

        /// <summary>
        /// Determines whether the specified content are equal.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="toCompare">To compare.</param>
        /// <returns>
        ///   <c>true</c> if the specified content are equal; otherwise, <c>false</c>.
        /// </returns>
        [DynamicExtension]
        public static bool IsEqualTo(this Content content, object toCompare)
        {
            var contentToCompare = ParseContentFromObject(toCompare);
            if (contentToCompare == null)
                return false;

            return contentToCompare.Id == content.Id;
        }

        /// <summary>
        /// Determines whether the specified content are equal.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="toCompare">To compare.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsEqualTo(this Content content, object toCompare, string valueIfTrue)
        {
            return content.IsEqualTo(toCompare, valueIfTrue, String.Empty);
        }

        /// <summary>
        /// Determines whether the specified content are equal.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="toCompare">To compare.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <param name="valueIfFalse">The value if false.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsEqualTo(this Content content, object toCompare, string valueIfTrue, string valueIfFalse)
        {
            return content.IsEqualTo(toCompare)
                       ? new HtmlString(valueIfTrue)
                       : new HtmlString(valueIfFalse);
        }

        #endregion

        #region IsNotEqualTo

        /// <summary>
        /// Determines whether the specified content are not equal.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="toCompare">To compare.</param>
        /// <returns>
        ///   <c>true</c> if the specified content are not equal; otherwise, <c>false</c>.
        /// </returns>
        [DynamicExtension]
        public static bool IsNotEqualTo(this Content content, object toCompare)
        {
            return !content.IsEqualTo(toCompare);
        }

        /// <summary>
        /// Determines whether the specified content are not equal.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="toCompare">To compare.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsNotEqualTo(this Content content, object toCompare, string valueIfTrue)
        {
            return content.IsNotEqualTo(toCompare, valueIfTrue, String.Empty);
        }

        /// <summary>
        /// Determines whether the specified content are not equal.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="toCompare">To compare.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <param name="valueIfFalse">The value if false.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsNotEqualTo(this Content content, object toCompare, string valueIfTrue, string valueIfFalse)
        {
            return content.IsNotEqualTo(toCompare)
                       ? new HtmlString(valueIfTrue)
                       : new HtmlString(valueIfFalse);
        }

        #endregion

        #region IsDescendantOf

        /// <summary>
        /// Determines whether the specified content is a descendant of the content to compare.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="toCompare">To compare.</param>
        /// <returns>
        ///   <c>true</c> if the specified content is a descendant of the content to compare; otherwise, <c>false</c>.
        /// </returns>
        [DynamicExtension]
        public static bool IsDescendantOf(this Content content, object toCompare)
        {
            var contentToCompare = ParseContentFromObject(toCompare);
            if (contentToCompare == null)
                return false;

            return contentToCompare.IsAncestorOf(content);
        }

        /// <summary>
        /// Determines whether the specified content is a descendant of the content to compare.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="toCompare">To compare.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsDescendantOf(this Content content, object toCompare, string valueIfTrue)
        {
            return content.IsDescendantOf(toCompare, valueIfTrue, String.Empty);
        }

        /// <summary>
        /// Determines whether the specified content is a descendant of the content to compare.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="toCompare">To compare.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <param name="valueIfFalse">The value if false.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsDescendantOf(this Content content, object toCompare, string valueIfTrue, string valueIfFalse)
        {
            return content.IsDescendantOf(toCompare)
                       ? new HtmlString(valueIfTrue)
                       : new HtmlString(valueIfFalse);
        }

        #endregion

        #region IsDescendantOfOrEqualTo

        /// <summary>
        /// Determines whether the specified content is a descendant of the content to compare or equal to it.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="toCompare">To compare.</param>
        /// <returns>
        ///   <c>true</c> if the specified content is a descendant of the content to compare or equal to itself; otherwise, <c>false</c>.
        /// </returns>
        [DynamicExtension]
        public static bool IsDescendantOfOrEqualTo(this Content content, object toCompare)
        {
            var contentToCompare = ParseContentFromObject(toCompare);
            if (contentToCompare == null)
                return false;

            return contentToCompare.IsAncestorOfOrEqualTo(content);
        }

        /// <summary>
        /// Determines whether the specified content is a descendant of the content to compare or equal to it.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="toCompare">To compare.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsDescendantOfOrEqualTo(this Content content, object toCompare, string valueIfTrue)
        {
            return content.IsDescendantOfOrEqualTo(toCompare, valueIfTrue, String.Empty);
        }

        /// <summary>
        /// Determines whether the specified content is a descendant of the content to compare or equal to it.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="toCompare">To compare.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <param name="valueIfFalse">The value if false.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsDescendantOfOrEqualTo(this Content content, object toCompare, string valueIfTrue, string valueIfFalse)
        {
            return content.IsDescendantOfOrEqualTo(toCompare)
                       ? new HtmlString(valueIfTrue)
                       : new HtmlString(valueIfFalse);
        }

        #endregion

        #region IsAncestorOf

        /// <summary>
        /// Determines whether the specified content is an ancestor of the content to compare.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="toCompare">To compare.</param>
        /// <returns>
        ///   <c>true</c> if the specified content is an ancestor of the content to compare; otherwise, <c>false</c>.
        /// </returns>
        [DynamicExtension]
        public static bool IsAncestorOf(this Content content, object toCompare)
        {
            var contentToCompare = ParseContentFromObject(toCompare);
            if (contentToCompare == null)
                return false;

            var hive = DependencyResolver.Current.GetService<IHiveManager>();
            return hive.FrameworkContext.ScopedCache.GetOrCreateTyped<bool>("cihe_IsAncestortOf_" + content.Id + "_" + contentToCompare.Id, () =>
            {
                using (var uow = hive.OpenReader<IContentStore>())
                {
                    var ancestorIds = uow.Repositories.GetAncestorIds(contentToCompare.Id, FixedRelationTypes.DefaultRelationType);
                    return ancestorIds.Any(x => x == content.Id);
                }
            });
        }

        /// <summary>
        /// Determines whether the specified content is an ancestor of the content to compare.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="toCompare">To compare.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsAncestorOf(this Content content, object toCompare, string valueIfTrue)
        {
            return content.IsAncestorOf(toCompare, valueIfTrue, String.Empty);
        }

        /// <summary>
        /// Determines whether the specified content is an ancestor of the content to compare.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="toCompare">To compare.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <param name="valueIfFalse">The value if false.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsAncestorOf(this Content content, object toCompare, string valueIfTrue, string valueIfFalse)
        {
            return content.IsAncestorOf(toCompare)
                       ? new HtmlString(valueIfTrue)
                       : new HtmlString(valueIfFalse);
        }

        #endregion

        #region IsAncestorOfOrEqualTo

        /// <summary>
        /// Determines whether the specified content is an ancestor of the content to compare or equal to it.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="toCompare">To compare.</param>
        /// <returns>
        ///   <c>true</c> if the specified content is an ancestor of the content to compare or equal to itself; otherwise, <c>false</c>.
        /// </returns>
        [DynamicExtension]
        public static bool IsAncestorOfOrEqualTo(this Content content, object toCompare)
        {
            var contentToCompare = ParseContentFromObject(toCompare);
            if (contentToCompare == null)
                return false;

            var hive = DependencyResolver.Current.GetService<IHiveManager>();
            return hive.FrameworkContext.ScopedCache.GetOrCreateTyped<bool>("cihe_IsAncestorOfOrEqualTo_" + content.Id + "_" + contentToCompare.Id, () =>
            {
                using (var uow = hive.OpenReader<IContentStore>())
                {
                    var ancestorIds = uow.Repositories.GetAncestorsIdsOrSelf(contentToCompare.Id, FixedRelationTypes.DefaultRelationType);
                    return ancestorIds.Any(x => x == content.Id);
                }
            });
        }

        /// <summary>
        /// Determines whether the specified content is an ancestor of the content to compare or equal to it.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="toCompare">To compare.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsAncestorOfOrEqualTo(this Content content, object toCompare, string valueIfTrue)
        {
            return content.IsAncestorOfOrEqualTo(toCompare, valueIfTrue, String.Empty);
        }

        /// <summary>
        /// Determines whether the specified content is an ancestor of the content to compare or equal to it.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="toCompare">To compare.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <param name="valueIfFalse">The value if false.</param>
        /// <returns></returns>
        [DynamicExtension]
        public static IHtmlString IsAncestorOfOrEqualTo(this Content content, object toCompare, string valueIfTrue, string valueIfFalse)
        {
            return content.IsAncestorOfOrEqualTo(toCompare)
                       ? new HtmlString(valueIfTrue)
                       : new HtmlString(valueIfFalse);
        }

        #endregion

        #region Obsolete

        #region IsModZero

        /// <summary>
        /// Determines whether the specified content's position is evenly divisible by the specified mod value.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="mod">The mod.</param>
        /// <returns>
        ///   <c>true</c> if the specified contents position is evenly divisible by the specified mod value; otherwise, <c>false</c>.
        /// </returns>
        [Obsolete("'IsModZero' is obsolete. Please use 'IsPositionDivisibleBy' instead.", true)]
        [DynamicExtension]
        public static bool IsModZero(this Content content, int mod)
        {
            throw new ObsoleteException("IsModZero", "IsPositionDivisibleBy");
        }

        /// <summary>
        /// Determines whether the specified content's position is evenly divisible by the specified mod value.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="mod">The mod.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <returns></returns>
        [Obsolete("'IsModZero' is obsolete. Please use 'IsPositionDivisibleBy' instead.", true)]
        [DynamicExtension]
        public static IHtmlString IsModZero(this Content content, int mod, string valueIfTrue)
        {
            throw new ObsoleteException("IsModZero", "IsPositionDivisibleBy");
        }

        /// <summary>
        /// Determines whether the specified content's position is evenly divisible by the specified mod value.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="mod">The mod.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <param name="valueIfFalse">The value if false.</param>
        /// <returns></returns>
        [Obsolete("'IsModZero' is obsolete. Please use 'IsPositionDivisibleBy' instead.", true)]
        [DynamicExtension]
        public static IHtmlString IsModZero(this Content content, int mod, string valueIfTrue, string valueIfFalse)
        {
            throw new ObsoleteException("IsModZero", "IsPositionDivisibleBy");
        }

        #endregion

        #region IsNotModZero

        /// <summary>
        /// Determines whether the specified content's position is not evenly divisible by the specified mod value.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="mod">The mod.</param>
        /// <returns>
        ///   <c>true</c> if the specified contents position is not evenly divisible by the specified mod value; otherwise, <c>false</c>.
        /// </returns>
        [Obsolete("'IsNotModZero' is obsolete. Please use 'IsPositionIndivisibleBy' instead.", true)]
        [DynamicExtension]
        public static bool IsNotModZero(this Content content, int mod)
        {
            throw new ObsoleteException("IsNotModZero", "IsPositionIndivisibleBy");
        }

        /// <summary>
        /// Determines whether the specified content's position is not evenly divisible by the specified mod value.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="mod">The mod.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <returns></returns>
        [Obsolete("'IsNotModZero' is obsolete. Please use 'IsPositionIndivisibleBy' instead.", true)]
        [DynamicExtension]
        public static IHtmlString IsNotModZero(this Content content, int mod, string valueIfTrue)
        {
            throw new ObsoleteException("IsNotModZero", "IsPositionIndivisibleBy");
        }

        /// <summary>
        /// Determines whether the specified content's position is not evenly divisible by the specified mod value.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="mod">The mod.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <param name="valueIfFalse">The value if false.</param>
        /// <returns></returns>
        [Obsolete("'IsNotModZero' is obsolete. Please use 'IsPositionIndivisibleBy' instead.", true)]
        [DynamicExtension]
        public static IHtmlString IsNotModZero(this Content content, int mod, string valueIfTrue, string valueIfFalse)
        {
            throw new ObsoleteException("IsNotModZero", "IsPositionIndivisibleBy");
        }

        #endregion

        #region IsEqual

        /// <summary>
        /// Determines whether the specified content are equal.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="toCompare">To compare.</param>
        /// <returns>
        ///   <c>true</c> if the specified content are equal; otherwise, <c>false</c>.
        /// </returns>
        [Obsolete("'IsEqual' is obsolete. Please use 'IsEqualTo' instead.", true)]
        [DynamicExtension]
        public static bool IsEqual(this Content content, object toCompare)
        {
            throw new ObsoleteException("IsEqual", "IsEqualTo");
        }

        /// <summary>
        /// Determines whether the specified content are equal.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="toCompare">To compare.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <returns></returns>
        [Obsolete("'IsEqual' is obsolete. Please use 'IsEqualTo' instead.", true)]
        [DynamicExtension]
        public static IHtmlString IsEqual(this Content content, object toCompare, string valueIfTrue)
        {
            throw new ObsoleteException("IsEqual", "IsEqualTo");
        }

        /// <summary>
        /// Determines whether the specified content are equal.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="toCompare">To compare.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <param name="valueIfFalse">The value if false.</param>
        /// <returns></returns>
        [Obsolete("'IsEqual' is obsolete. Please use 'IsEqualTo' instead.", true)]
        [DynamicExtension]
        public static IHtmlString IsEqual(this Content content, object toCompare, string valueIfTrue, string valueIfFalse)
        {
            throw new ObsoleteException("IsEqual", "IsEqualTo");
        }

        #endregion

        #region IsDescendant

        /// <summary>
        /// Determines whether the specified content is a descendant of the content to compare.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="toCompare">To compare.</param>
        /// <returns>
        ///   <c>true</c> if the specified content is a descendant of the content to compare; otherwise, <c>false</c>.
        /// </returns>
        [Obsolete("'IsDescendant' is obsolete. Please use 'IsDescendantOf' instead.", true)]
        [DynamicExtension]
        public static bool IsDescendant(this Content content, object toCompare)
        {
            throw new ObsoleteException("IsDescendant", "IsDescendantOf");
        }

        /// <summary>
        /// Determines whether the specified content is a descendant of the content to compare.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="toCompare">To compare.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <returns></returns>
        [Obsolete("'IsDescendant' is obsolete. Please use 'IsDescendantOf' instead.", true)]
        [DynamicExtension]
        public static IHtmlString IsDescendant(this Content content, object toCompare, string valueIfTrue)
        {
            throw new ObsoleteException("IsDescendant", "IsDescendantOf");
        }

        /// <summary>
        /// Determines whether the specified content is a descendant of the content to compare.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="toCompare">To compare.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <param name="valueIfFalse">The value if false.</param>
        /// <returns></returns>
        [Obsolete("'IsDescendant' is obsolete. Please use 'IsDescendantOf' instead.", true)]
        [DynamicExtension]
        public static IHtmlString IsDescendant(this Content content, object toCompare, string valueIfTrue, string valueIfFalse)
        {
            throw new ObsoleteException("IsDescendant", "IsDescendantOf");
        }

        #endregion

        #region IsDescendantOrSelf

        /// <summary>
        /// Determines whether the specified content is a descendant of the content to compare or equal to it.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="toCompare">To compare.</param>
        /// <returns>
        ///   <c>true</c> if the specified content is a descendant of the content to compare or equal to itself; otherwise, <c>false</c>.
        /// </returns>
        [Obsolete("'IsDescendantOrSelf' is obsolete. Please use 'IsDescendantOfOrEqualTo' instead.", true)]
        [DynamicExtension]
        public static bool IsDescendantOrSelf(this Content content, object toCompare)
        {
            throw new ObsoleteException("IsDescendantOrSelf", "IsDescendantOfOrEqualTo");
        }

        /// <summary>
        /// Determines whether the specified content is a descendant of the content to compare or equal to it.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="toCompare">To compare.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <returns></returns>
        [Obsolete("'IsDescendantOrSelf' is obsolete. Please use 'IsDescendantOfOrEqualTo' instead.", true)]
        [DynamicExtension]
        public static IHtmlString IsDescendantOrSelf(this Content content, object toCompare, string valueIfTrue)
        {
            throw new ObsoleteException("IsDescendantOrSelf", "IsDescendantOfOrEqualTo");
        }

        /// <summary>
        /// Determines whether the specified content is a descendant of the content to compare or equal to it.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="toCompare">To compare.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <param name="valueIfFalse">The value if false.</param>
        /// <returns></returns>
        [Obsolete("'IsDescendantOrSelf' is obsolete. Please use 'IsDescendantOfOrEqualTo' instead.", true)]
        [DynamicExtension]
        public static IHtmlString IsDescendantOrSelf(this Content content, object toCompare, string valueIfTrue, string valueIfFalse)
        {
            throw new ObsoleteException("IsDescendantOrSelf", "IsDescendantOfOrEqualTo");
        }

        #endregion

        #region IsAncestor

        /// <summary>
        /// Determines whether the specified content is an ancestor of the content to compare.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="toCompare">To compare.</param>
        /// <returns>
        ///   <c>true</c> if the specified content is an ancestor of the content to compare; otherwise, <c>false</c>.
        /// </returns>
        [Obsolete("'IsAncestor' is obsolete. Please use 'IsAncestorOf' instead.", true)]
        [DynamicExtension]
        public static bool IsAncestor(this Content content, object toCompare)
        {
            throw new ObsoleteException("IsAncestor", "IsAncestorOf");
        }

        /// <summary>
        /// Determines whether the specified content is an ancestor of the content to compare.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="toCompare">To compare.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <returns></returns>
        [Obsolete("'IsAncestor' is obsolete. Please use 'IsAncestorOf' instead.", true)]
        [DynamicExtension]
        public static IHtmlString IsAncestor(this Content content, object toCompare, string valueIfTrue)
        {
            throw new ObsoleteException("IsAncestor", "IsAncestorOf");
        }

        /// <summary>
        /// Determines whether the specified content is an ancestor of the content to compare.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="toCompare">To compare.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <param name="valueIfFalse">The value if false.</param>
        /// <returns></returns>
        [Obsolete("'IsAncestor' is obsolete. Please use 'IsAncestorOf' instead.", true)]
        [DynamicExtension]
        public static IHtmlString IsAncestor(this Content content, object toCompare, string valueIfTrue, string valueIfFalse)
        {
            throw new ObsoleteException("IsAncestor", "IsAncestorOf");
        }

        #endregion

        #region IsAncestorOrSelf

        /// <summary>
        /// Determines whether the specified content is an ancestor of the content to compare or equal to it.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="toCompare">To compare.</param>
        /// <returns>
        ///   <c>true</c> if the specified content is an ancestor of the content to compare or equal to itself; otherwise, <c>false</c>.
        /// </returns>
        [Obsolete("'IsAncestorOrSelf' is obsolete. Please use 'IsAncestorOfOrEqualTo' instead.", true)]
        [DynamicExtension]
        public static bool IsAncestorOrSelf(this Content content, object toCompare)
        {
            throw new ObsoleteException("IsAncestorOrSelf", "IsAncestorOfOrEqualTo");
        }

        /// <summary>
        /// Determines whether the specified content is an ancestor of the content to compare or equal to it.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="toCompare">To compare.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <returns></returns>
        [Obsolete("'IsAncestorOrSelf' is obsolete. Please use 'IsAncestorOfOrEqualTo' instead.", true)]
        [DynamicExtension]
        public static IHtmlString IsAncestorOrSelf(this Content content, object toCompare, string valueIfTrue)
        {
            throw new ObsoleteException("IsAncestorOrSelf", "IsAncestorOfOrEqualTo");
        }

        /// <summary>
        /// Determines whether the specified content is an ancestor of the content to compare or equal to it.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="toCompare">To compare.</param>
        /// <param name="valueIfTrue">The value if true.</param>
        /// <param name="valueIfFalse">The value if false.</param>
        /// <returns></returns>
        [Obsolete("'IsAncestorOrSelf' is obsolete. Please use 'IsAncestorOfOrEqualTo' instead.", true)]
        [DynamicExtension]
        public static IHtmlString IsAncestorOrSelf(this Content content, object toCompare, string valueIfTrue, string valueIfFalse)
        {
            throw new ObsoleteException("IsAncestorOrSelf", "IsAncestorOfOrEqualTo");
        }

        #endregion

        #endregion

        #region Helper Methods

        private static Content ParseContentFromObject(object obj)
        {
            var content = obj as Content;
            if (content == null)
            {
                var bendyComparer = obj as BendyObject;
                if (bendyComparer != null)
                {
                    try
                    {
                        if (bendyComparer["__OriginalItem"] != null && bendyComparer["__OriginalItem"] is Content)
                            content = ((Content)bendyComparer["__OriginalItem"]);
                    }
                    catch (Exception)
                    {
                        // Do nothing
                    }
                }
            }

            return content;
        }

        #endregion
    }
}
