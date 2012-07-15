using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Rebel.Cms.Web.Model;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model.Attribution;
using Rebel.Framework.Persistence.ModelFirst;
using Rebel.Hive;

namespace Rebel.Cms.Web.Mapping
{
    internal class SimpleFlattenedTypedEntityMapper
    {
        private const string UploaderType = "Uploader";
        private const string MediaPickerType = "Media Picker";
        private readonly IHiveManager _hiveManager;
        private readonly string _urlPostfix;
        private readonly UrlHelper _url;

        internal SimpleFlattenedTypedEntityMapper(IHiveManager hiveManager, UrlHelper url, string urlPostfix)
        {
            _hiveManager = hiveManager;
            _url = url;
            _urlPostfix = urlPostfix;
        }

        internal object Flatten(string itemUrl, CustomTypedEntity<Content> typedEntity)
        {
            Dictionary<string, object> properties = ExtractProperties(typedEntity);
            IEnumerable<HiveId> children = typedEntity.AllChildIds();
            
            List<string> childrenUrls = children
                .Select(child => _hiveManager.Cms().Content.GetById(child))
                .Select(entity => string.Concat(entity.NiceUrl(),_urlPostfix))
                .ToList();

            return new { 
                        id = itemUrl,
                        UtcCreated = ConvertDateTime(typedEntity.UtcCreated), 
                        UtcModified = ConvertDateTime(typedEntity.UtcModified), 
                        UtcStatusChanged = ConvertDateTime(typedEntity.UtcStatusChanged), 
                        properties, 
                        children = childrenUrls 
            };
        }

        private string ConvertDateTime(DateTimeOffset toConvert)
        {
            return toConvert.ToString("dd/MM/yyyy hh:mm:ss");
        }

        private Dictionary<string, object> ExtractProperties(CustomTypedEntity<Content> typedEntity)
        {
            var dictionary = new Dictionary<string, object>();
            foreach (TypedAttribute attribute in typedEntity.Attributes)
            {
                string key = attribute.AttributeDefinition.Alias;
                if (key.Contains("system")) continue;
                LocalizedString type = attribute.AttributeDefinition.AttributeType.Name;
                dynamic value = SetPropertyValue(typedEntity, key, attribute, type);
                dictionary.Add(key, value);
            }
            return dictionary;
        }

        private dynamic SetPropertyValue(CustomTypedEntity<Content> typedEntity, string key, TypedAttribute attribute,
                                         LocalizedString type)
        {
            dynamic value = attribute.DynamicValue;
            if (type == UploaderType)
            {
                value = _url.GetMediaUrl(typedEntity, key);
            }
            else if (type == MediaPickerType)
            {
                string id = string.Concat(string.Empty, value);
                if (!string.IsNullOrEmpty(id))
                {
                    value = _url.GetMediaUrl(id, "uploadedFile");
                }
            }
            return value;
        }
    }
}