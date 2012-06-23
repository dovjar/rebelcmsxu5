using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using RebelCms.Cms.Web.EmbeddedViewEngine;
using RebelCms.Cms.Web.Model.BackOffice.PropertyEditors;

namespace RebelCms.Cms.Web.PropertyEditors.RebelCmsSystemEditors.DictionaryItemTranslations
{
    [Bind(Exclude = "LanguageCodes")]
    [ModelBinder(typeof(DictionaryItemTranslationsEditorModelBinder))]
    [EmbeddedView("RebelCms.Cms.Web.PropertyEditors.RebelCmsSystemEditors.DictionaryItemTranslations.Views.DictionaryItemTranslationsEditor.cshtml", "RebelCms.Cms.Web.PropertyEditors")]
    public class DictionaryItemTranslationsEditorModel : EditorModel
    {
        public DictionaryItemTranslationsEditorModel(IDictionary<string, string> languages)
        {
            Languages = languages;
        }

        public override bool ShowRebelCmsLabel { get { return false; } }

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
