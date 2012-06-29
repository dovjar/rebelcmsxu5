using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants.SerializationTypes;

namespace Umbraco.Framework.Persistence.Model.Constants.AttributeTypes
{
    public class DictionaryItemTranslationsAttributeType : AttributeType
    {
        public const string AliasValue = "system-dictionary-item-translations-type";

        public DictionaryItemTranslationsAttributeType()
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
