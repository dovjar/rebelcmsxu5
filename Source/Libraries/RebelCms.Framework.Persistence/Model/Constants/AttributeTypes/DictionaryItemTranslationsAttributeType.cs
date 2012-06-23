using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RebelCms.Framework.Persistence.Model.Attribution.MetaData;
using RebelCms.Framework.Persistence.Model.Constants.SerializationTypes;

namespace RebelCms.Framework.Persistence.Model.Constants.AttributeTypes
{
    public class DictionaryItemTranslationsAttributeType : AttributeType
    {
        public const string AliasValue = "system-dictionary-item-translations-type";

        internal DictionaryItemTranslationsAttributeType()
            : base(
            AliasValue,
            AliasValue, 
            "This type represents internal system dictionary items translations", 
            new StringSerializationType())
        {
            Id = FixedHiveIds.DictionaryItemTranslationsAttributeType;
        }
    }
}
