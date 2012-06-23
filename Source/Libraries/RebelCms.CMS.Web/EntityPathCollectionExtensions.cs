using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using RebelCms.Framework;
using RebelCms.Framework.Persistence.Model;
using RebelCms.Framework.Persistence.Model.Associations;
using RebelCms.Framework.Persistence.Model.Attribution.MetaData;
using RebelCms.Hive;
using RebelCms.Hive.ProviderGrouping;
using RebelCms.Hive.RepositoryTypes;

namespace RebelCms.Cms.Web
{
    public static class EntityPathCollectionExtensions
    {
        /// <summary>
        /// Converts an entity path collection to a JSON object
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static JArray ToJson(this EntityPathCollection paths)
        {
            return new JArray(paths.Select(x => new JArray(x.Select(y => y.ToJsonObject()))));
        }
    }
}
