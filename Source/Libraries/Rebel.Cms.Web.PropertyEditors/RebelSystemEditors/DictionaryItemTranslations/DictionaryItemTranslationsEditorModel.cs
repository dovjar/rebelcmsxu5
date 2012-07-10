using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Rebel.Cms.Web.EmbeddedViewEngine;
using Rebel.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Rebel.Cms.Web.PropertyEditors.RebelSystemEditors.DictionaryItemTranslations
{
    [Bind(Exclude = "LanguageCodes")]
    [ModelBinder(typeof(DictionaryItemTranslationsEditorModelBinder))]
    [EmbeddedView("Rebel.Cms.Web.PropertyEditors.RebelSystemEditors.DictionaryItemTranslations.Views.DictionaryItemTranslationsEditor.cshtml", "Rebel.Cms.Web.PropertyEditors")]
    public class DictionaryItemTranslationsEditorModel : EditorModel
    {
        public DictionaryItemTranslationsEditorModel(IDictionary<string, string> languages)
        {
            Languages = languages;
        }

        public override bool ShowRebelLabel { get { return false; } }

        [ScaffoldColumn(false)]
        [ReadOnly(true)]
        public IDictionary<string, string> Languages { get; private set; }

        public IDictionary<string, string> Translations { get; set; }

        public override IDictionary<string, object> GetSerializedValue()
        {
            return Translations.ToDictionary(k => k.Key, v => (object)v.Value);
        }

        public override void SetModelValues(IDictionary<string, object> serializedVal)
        {
            Translations = serializedVal.ToDictionary(k => k.Key, v => v.Value.ToString());
        }
    }
}
