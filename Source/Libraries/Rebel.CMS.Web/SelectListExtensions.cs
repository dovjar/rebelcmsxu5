using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Rebel.Cms.Web
{

    /// <summary>
    /// <![CDATA[
    /// Extension methods for SelectList and IEnumerable<SelectListItem>
    /// ]]>
    /// </summary>
    public static class SelectListExtensions
    {

        public static SelectList ToSelectList(this IEnumerable<SelectListItem> collection)
        {
            return new SelectList(collection, "Text", "Value");
        }

        public static SelectList ToSelectList(this IEnumerable<SelectListItem> collection, object selectedValue)
        {
            return new SelectList(collection, "Text", "Value", selectedValue);
        }

        public static SelectList ToSelectList(this IDictionary<object, object> collection)
        {
            return new SelectList(collection, "Key", "Value");
        }

        public static SelectList ToSelectList(this IDictionary<object, object> collection, object selectedValue)
        {
            return new SelectList(collection, "Key", "Value", selectedValue);
        }

        public static SelectList ToSelectList<T>(this IEnumerable<T> collection, string dataValueField, string dataTextField)
        {
            return new SelectList(collection, dataValueField, dataTextField);
        }

        public static SelectList ToSelectList<T>(this IEnumerable<T> collection, string dataValueField, string dataTextField, object selectedValue)
        {
            return new SelectList(collection, dataValueField, dataTextField, selectedValue);
        }

        /// <summary>
        /// Un-selects all items in list
        /// </summary>
        /// <param name="selectList"></param>
        public static void UnSelectItems(this IEnumerable<SelectListItem> selectList)
        {
            foreach(var i in selectList)
            {
                i.Selected = false;
            }
        }

        /// <summary>
        /// Select all items that have values contained in selectedVals
        /// </summary>
        /// <param name="selectList"></param>
        /// <param name="selectedVals"></param>
        public static void SelectItems(this IEnumerable<SelectListItem> selectList, IEnumerable<string> selectedVals)
        {
            foreach (var i in selectList)
            {
                i.Selected = selectedVals.Contains(i.Value);
            }
        }

        public static void SelectItems(this IEnumerable<SelectListItem> selectList, params string[] selectedVals)
        {
            foreach (var i in selectList)
            {
                i.Selected = selectedVals.Contains(i.Value);
            }
        }

    }
}
