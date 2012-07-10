using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Framework.Persistence.Model.Constants.AttributeDefinitions;

namespace Rebel.Cms.Web.Model.BackOffice.Editors
{
    public class DictionaryItemEditorModel : StandardContentEditorModel
    {
        public string Key
        {
            get { return GetPropertyEditorModelValue(NodeNameAttributeDefinition.AliasValue, x => x.UrlName); }
            set { SetPropertyEditorModelValue(NodeNameAttributeDefinition.AliasValue, x => x.UrlName = value); }
        }
    }
}
