using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Rebel.Cms.Web.Model.BackOffice.ParameterEditors
{
    public class EditorModel
    {
        private ModelMetadata _modelMetadata;

        /// <summary>
        /// Gets the property editor model value key.
        /// </summary>
        public virtual string PropertyEditorModelValueKey
        {
            get { return "Value"; }
        }

        /// <summary>
        /// Gets the property editor model.
        /// </summary>
        public dynamic PropertyEditorModel { get; set; }

        /// <summary>
        /// A flag determining if the editor rendered shows the Rebel standard field label, or
        /// if this model's editor will occupy the area where the label generally exists such as the rich text editor.
        /// </summary>
        [ReadOnly(true)]
        [ScaffoldColumn(false)]
        public virtual bool ShowRebelLabel
        {
            get { return PropertyEditorModel.ShowRebelLabel; }
        }

        /// <summary>
        /// Returns the meta data for the current editor model
        /// </summary>
        protected internal ModelMetadata MetaData
        {
            get
            {
                return _modelMetadata ?? (_modelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => this, GetType()));
            }
        }

        /// <summary>
        /// Gets the serialized value.
        /// </summary>
        /// <returns></returns>
        public virtual string GetSerializedValue()
        {
            var propEditorSerializedValues = PropertyEditorModel.GetSerializedValue();
            if (!propEditorSerializedValues.ContainsKey(PropertyEditorModelValueKey))
                throw new ApplicationException("Property Editor Model does not contain the value key '" + PropertyEditorModelValueKey + "'");
            return propEditorSerializedValues[PropertyEditorModelValueKey] != null ? propEditorSerializedValues[PropertyEditorModelValueKey].ToString() : "";
        }

        /// <summary>
        /// Sets the model value.
        /// </summary>
        /// <param name="serializedVal">The serialized val.</param>
        public virtual void SetModelValue(string serializedVal)
        {
            if (!string.IsNullOrEmpty(serializedVal))
            {
                var propEditorSerializedValues = new Dictionary<string, object>
                                                     {{PropertyEditorModelValueKey, serializedVal}};
                PropertyEditorModel.SetModelValues(propEditorSerializedValues);
            }
        }
    }
}
