using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml.Linq;
using Rebel.Cms.Web.EmbeddedViewEngine;
using Rebel.Cms.Web.Model.BackOffice.Editors;
using Rebel.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Rebel.Cms.Web.PropertyEditors.TypePicker
{
    [Bind(Exclude = "Type")]
    [ModelBinder(typeof(TypePickerPreValueModelBinder))]
    public class TypePickerPreValueModel  : PreValueModel
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is required.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is required; otherwise, <c>false</c>.
        /// </value>
        [AllowDocumentTypePropertyOverride]
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        [ReadOnly(true)]
        [ScaffoldColumn(false)]
        public TypePickerType Type 
        {
            get
            {
                var value = AvailableTypes.Where(x => x.Selected).Select(x => x.Value).FirstOrDefault();
                return value == null ? TypePickerType.Document : (TypePickerType)Enum.Parse(typeof(TypePickerType), value);
            }
            set
            {
                var item = AvailableTypes.SingleOrDefault(x => x.Value == value.ToString());
                if (item != null)
                    item.Selected = true;
            }
        }

        /// <summary>
        /// Gets or sets the available types.
        /// </summary>
        /// <value>
        /// The available types.
        /// </value>
        [Display(Name = "Type")]
        [EmbeddedView("Rebel.Cms.Web.PropertyEditors.TypePicker.Views.TypePickerDropDown.cshtml", "Rebel.Cms.Web.PropertyEditors")]
        public IEnumerable<SelectListItem> AvailableTypes { get; set; }

        /// <summary>
        /// Gets the available types.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SelectListItem> GetAvailableTypes()
        {
            return Enum.GetNames(typeof (TypePickerType)).Select(x => new SelectListItem {Text = x, Value = x}).ToList();
        }

        /// <summary>
        /// Get the list of Ids and create the select list
        /// </summary>
        /// <param name="serializedVal"></param>
        public override void SetModelValues(string serializedVal)
        {
            AvailableTypes = GetAvailableTypes();

            if (!string.IsNullOrWhiteSpace(serializedVal))
            {
                var xml = XElement.Parse(serializedVal);

                var typeEl = xml.Elements("preValue").SingleOrDefault(x => (string)x.Attribute("name") == "Type");
                if (typeEl != null)
                {
                    Type = (TypePickerType)Enum.Parse(typeof(TypePickerType), typeEl.Value);
                }

                IsRequired = xml.Elements("preValue").Any(x => (string)x.Attribute("name") == "IsRequired" && x.Value == bool.TrueString);
            }
        }

        /// <summary>
        /// Return a serialized string of values for the pre value editor model
        /// </summary>
        /// <returns></returns>
        public override string GetSerializedValue()
        {
            var xml = new XElement("preValues",
                new XElement("preValue", new XAttribute("name", "Type"), new XCData(Type.ToString())),
                new XElement("preValue", new XAttribute("name", "IsRequired"), new XCData(IsRequired.ToString())));

            return xml.ToString();
        }
    }
}
