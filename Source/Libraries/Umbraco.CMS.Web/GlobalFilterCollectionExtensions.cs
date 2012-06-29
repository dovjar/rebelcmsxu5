using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Umbraco.Cms.Web
{
    public static class GlobalFilterCollectionExtensions
    {

        /// <summary>
        /// Checks if the GlobalFilters contains a filter of a specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filters"></param>
        /// <returns></returns>
        public static bool ContainsFilter<T>(this GlobalFilterCollection filters)
        {
            var instanceTypes = filters.Select(x => x.Instance);
            return instanceTypes.OfType<T>().Any();
        }
    }
}
