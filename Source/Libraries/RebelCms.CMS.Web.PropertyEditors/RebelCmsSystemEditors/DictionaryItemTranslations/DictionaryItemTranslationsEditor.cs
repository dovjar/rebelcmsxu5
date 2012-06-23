using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Model.BackOffice.PropertyEditors;

namespace RebelCms.Cms.Web.PropertyEditors.RebelCmsSystemEditors.DictionaryItemTranslations
{
    [RebelCmsPropertyEditor]
    [PropertyEditor(CorePluginConstants.DictionaryItemTranslationsPropertyEditorId, "DictionaryItemTranslationsEditor", "Dictionary Item Translations Editor")]
    public class DictionaryItemTranslationsEditor : PropertyEditor<DictionaryItemTranslationsEditorModel>
    {
        private IRebelCmsApplicationContext _appContext;

        public DictionaryItemTranslationsEditor(IRebelCmsApplicationContext appContext)
        {
            _appContext = appContext;
        }

        public override DictionaryItemTranslationsEditorModel CreateEditorModel()
        {
            return new DictionaryItemTranslationsEditorModel(_appContext.Settings.Languages.ToDictionary(k => k.IsoCode, v => v.Name));
        }
    }
}
