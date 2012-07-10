using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Associations;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Hive;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Cms.Web
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
