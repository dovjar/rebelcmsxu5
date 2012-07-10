using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Rebel.Cms.Web.EmbeddedViewEngine;
using Rebel.Cms.Web.Model.BackOffice;
using Rebel.Cms.Web.Model.BackOffice.PropertyEditors;
using Rebel.Cms.Web.Mvc.Metadata;

namespace Rebel.Cms.Web.PropertyEditors.Tags
{
    [EmbeddedView("Rebel.Cms.Web.PropertyEditors.Tags.Views.TagsEditor.cshtml", "Rebel.Cms.Web.PropertyEditors")]
    public class TagsEditorModel : EditorModel<TagsPreValueModel>, IValidatableObject
    {
        public TagsEditorModel(TagsPreValueModel preValueModel)
            : base(preValueModel)
        { }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [ShowLabel(false)]
        public string Value { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PreValueModel.IsRequired && string.IsNullOrEmpty(Value))
            {
                yield return new ValidationResult("Value is required", new[] { "Value" });
            }
        }
    }
}
