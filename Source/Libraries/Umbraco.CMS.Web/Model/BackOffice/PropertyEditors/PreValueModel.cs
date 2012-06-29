using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using Umbraco.Framework;
using Umbraco.Framework.Diagnostics;

namespace Umbraco.Cms.Web.Model.BackOffice.PropertyEditors
{
    /// <summary>
    /// Abstract class representing a Property Editor's model to render it's Pre value editor
    /// </summary>
    public abstract class PreValueModel
    {                     

        private ModelMetadata _modelMetadata;
        
        /// <summary>
        /// Returns the meta data for the current pre value model
        /// </summary>
        protected ModelMetadata MetaData
        {
            get
            {
                return _modelMetadata ?? (_modelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => this, GetType()));
            }
        }

        /// <summary>
        /// Returns a collection of PreValueDefinition objects, each representing an individual value to be saved.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This collection will be turned in to the string returned for the result of GetSerializedValue()
        /// </remarks>
        protected virtual IEnumerable<PreValueDefinition> GetValueDefinitions()
        {
            //get all editable properties
            var editableProps = MetaData.Properties.Where(x => x.ShowForEdit);
            var definitions = new List<PreValueDefinition>();
            foreach (var p in editableProps)
            {
                //by default, we will not support complex modelled properties, developers will need to override
                //the GetSerializedValue method if they need support for this.
                if (p.IsComplexType)
                {
                    //TODO: We should magically support this
                    throw new NotSupportedException("The default serialization implementation of PreValueModel does not support properties that are complex models");
                }

                definitions.Add(new PreValueDefinition(
                    p.PropertyName,
                    p.ModelType,
                    p.Model));
            }
            return definitions;
        }

        /// <summary>
        /// Return a serialized string of values for the pre value editor model
        /// </summary>
        /// <returns></returns>
        public virtual string GetSerializedValue()
        {
            var xmlBody = new XElement("preValues");
            var defs = GetValueDefinitions();
            foreach(var d in defs)
            {                
                var output = d.ModelValue.TryConvertToXmlString(d.ModelType);
                if (!output.Success)
                {
                    output = d.ModelValue.TryConvertTo<string>();
                    if (!output.Success)
                    {
                        throw new NotSupportedException("Could not convert the value of " + d.PropertyName + " to a string representation");   
                    }
                }

                var xmlItem = new XElement("preValue",
                                 new XAttribute("name", d.PropertyName),
                                 new XAttribute("type", d.ModelType.FullName),
                                 new XCData(output.Result));

                xmlBody.Add(xmlItem);
            }
            return xmlBody.ToString();
        }
            
    
        /// <summary>
        /// Called to set each model property value from the serialized value when SetModelValues is called.
        /// </summary>
        /// <remarks>
        /// This allows devs to do any custom processing on the value before it is set if they are not 
        /// implementing their own custom serialization.
        /// </remarks>
        protected virtual void SetModelPropertyValue(PreValueDefinition def, Action<object> setProperty)
        {            
            setProperty(def.ModelValue);                
        }

        /// <summary>
        /// called by the subsystem to load the values from the data store into the model
        /// </summary>
        /// <param name="serializedVal"></param>
        public virtual void SetModelValues(string serializedVal)
        {
            if (string.IsNullOrEmpty(serializedVal))
            {
                return;
            }

            var xml = XElement.Parse(serializedVal);
            var modelProperties = GetType().GetProperties();
            if (xml.Name != "preValues")
            {
                throw new XmlException("The XML format for the serialized value is invalid");
            }
            foreach (var xmlPreValue in xml.Elements("preValue"))
            {
                if (xmlPreValue.Attribute("name") != null)
                {
                    //get the property with the name
                    var prop = modelProperties.Where(x => x.Name == (string)xmlPreValue.Attribute("name")).SingleOrDefault();
                    if (prop != null)
                    {
                        //set the property value
                        var converter = TypeDescriptor.GetConverter(prop.PropertyType);
                        if (converter != null)
                        {
                            try
                            {
                                var valToSet = converter.ConvertFromString(xmlPreValue.Value);

                                SetModelPropertyValue(
                                    new PreValueDefinition(prop.Name, prop.PropertyType, valToSet),
                                    x => prop.SetValue(this, x, null));
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Error<PreValueModel>("Could not set the model property value for " + prop.Name, ex);
                            }                            
                        }
                        
                    }
                }
            }
        }
    }
}
