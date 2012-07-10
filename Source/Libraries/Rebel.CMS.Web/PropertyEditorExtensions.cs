using System;
using System.Collections.Generic;
using System.Linq;
using Rebel.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Rebel.Cms.Web
{
    public static class PropertyEditorExtensions
    {

        /// <summary>
        /// Returns a property editor from the property editor factory using a string id which must parse to a Guid
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Lazy<PropertyEditor, PropertyEditorMetadata> GetPropertyEditor(this IPropertyEditorFactory factory, string id)
        {
            Guid propEditorId;
            if (!Guid.TryParse(id, out propEditorId))
                throw new InvalidCastException("The id specified is not a valid GUID");
            return factory.GetPropertyEditor(propEditorId);
        }

        /// <summary>
        /// Returns a property editor definition based on a property editor id
        /// </summary>
        /// <param name="propertyEditors"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Lazy<PropertyEditor, PropertyEditorMetadata> GetPropertyEditor(this IEnumerable<Lazy<PropertyEditor, PropertyEditorMetadata>> propertyEditors, Guid id)
        {
            return propertyEditors.Where(x => x.Metadata.Id == id).SingleOrDefault();
        }

        /// <summary>
        /// Converts a list of real property editors to property editor definitions
        /// </summary>
        /// <param name="props"></param>
        /// <returns></returns>
        /// <remarks>
        /// TODO: This is a helper class which is used for tests and demo data, this may not be required in the future except for unit tests
        /// </remarks>
        internal static IEnumerable<Lazy<PropertyEditor, PropertyEditorMetadata>> ToPropertyEditorDefinitions(this IEnumerable<PropertyEditor> props)
        {
            //convert to Lazy<PropertyEditor, PropertyEditorMetadata>
            return (from p in props
                    let prop = p
                    let data = new Dictionary<string, object>
                                                 {
                                                     { "Name", p.Name }, 
                                                     { "Alias", p.Alias },
                                                     { "Id", p.Id },
                                                     { "ComponentType", p.GetType()},
                                                     { "PluginDefinition", null }
                                                 }
                    select new Lazy<PropertyEditor, PropertyEditorMetadata>(() => prop, new PropertyEditorMetadata(data))).ToList();
        }
    }
}
