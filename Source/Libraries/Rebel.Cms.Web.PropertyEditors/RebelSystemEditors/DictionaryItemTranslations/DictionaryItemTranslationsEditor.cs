using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Rebel.Cms.Web.PropertyEditors.RebelSystemEditors.DictionaryItemTranslations
{
    [RebelPropertyEditor]
    [PropertyEditor(CorePluginConstants.DictionaryItemTranslationsPropertyEditorId, "DictionaryItemTranslationsEditor", "Dictionary Item Translations Editor")]
    public class DictionaryItemTranslationsEditor : PropertyEditor<DictionaryItemTranslationsEditorModel>
    {
        private IRebelApplicationContext _appContext;

        public DictionaryItemTranslationsEditor(IRebelApplicationContext appContext)
        {
            _appContext = appContext;
        }

        public override DictionaryItemTranslationsEditorModel CreateEditorModel()
        {
            return new DictionaryItemTranslationsEditorModel(_appContext.Settings.Languages.ToDictionary(k => k.IsoCode, v => v.Name));
        }
    }
}
