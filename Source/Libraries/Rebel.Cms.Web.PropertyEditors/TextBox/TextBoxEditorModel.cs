﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Rebel.Cms.Web.EmbeddedViewEngine;
using Rebel.Cms.Web.Model.BackOffice;
using Rebel.Cms.Web.Model.BackOffice.PropertyEditors;
using Rebel.Cms.Web.Model.BackOffice.UIElements;

namespace Rebel.Cms.Web.PropertyEditors.TextBox
{
    [EmbeddedView("Rebel.Cms.Web.PropertyEditors.TextBox.Views.TextBoxEditor.cshtml", "Rebel.Cms.Web.PropertyEditors")]
    public class TextBoxEditorModel : EditorModel<TextBoxPreValueModel>, IValidatableObject
    {
        public TextBoxEditorModel(TextBoxPreValueModel preValueModel)
            : base(preValueModel)
        {
            if (PreValueModel.Mode == TextBoxMode.MultiLineWithControls)
            {
                var uiElements = new List<UIElement>
                    {
                        new SeperatorUIElement(),
                        new ButtonUIElement
                            {
                                Alias = "Bold", 
                                Title = "Bold", 
                                CssClass = "text-box-bold-button"
                            },
                        new ButtonUIElement
                            {
                                Alias = "Italic", 
                                Title = "Italic", 
                                CssClass = "text-box-italic-button"
                            },
                        new ButtonUIElement
                            {
                                Alias = "Link", 
                                Title = "Link", 
                                CssClass = "text-box-link-button"
                            }
                    };

                UIElements = uiElements;
            }
        }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [ShowLabel(false)]
        public string Value { get; set; }

        /// <summary>
        /// Performs the dynamic validation against the pre-value model
        /// </summary>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //ensure the string is empty and never 'null'
            if (string.IsNullOrEmpty(Value)) Value = string.Empty; 

            if (PreValueModel.IsRequired && string.IsNullOrEmpty(Value))
            {
                yield return new ValidationResult("Value is required", new[] {"Value"});
            }
            
            if (!string.IsNullOrWhiteSpace(Value) && !string.IsNullOrEmpty(PreValueModel.RegexValidationStatement) && !Regex.IsMatch(Value, PreValueModel.RegexValidationStatement))
            {
                yield return new ValidationResult("Value does not match the required pattern", new[] { "Value" });
            }
        }
    }
}
