using System;
using Rebel.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Rebel.Cms.Web
{
    /// <summary>
    /// A Factory used to resolve PropertyEditors
    /// </summary>
    public interface IPropertyEditorFactory
    {
        Lazy<PropertyEditor, PropertyEditorMetadata> GetPropertyEditor(Guid id);
    }
}