using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.PropertyEditors.NodeSelector
{
    /// <summary>
    /// A Model representing the data required to display the previously persisted items including the name, id, icon/style
    /// </summary>    
    public class SelectedItemModel
    {
        public string NodeName { get; set; } 
        
        [JsonConverter(typeof(HiveIdJsonConverter))]
        public HiveId NodeId { get; set; }       
 
        public string NodeIcon { get; set; }
        public string NodeStyle { get; set; }

        /// <summary>
        /// The image url of the item if we are showing thumbnails
        /// </summary>
        public string ThumbnailUrl { get; set; }
    }
}