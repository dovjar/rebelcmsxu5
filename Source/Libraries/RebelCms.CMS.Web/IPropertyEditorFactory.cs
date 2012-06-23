using System;
using RebelCms.Cms.Web.Model.BackOffice.PropertyEditors;

namespace RebelCms.Cms.Web
{
    /// <summary>
    /// A Factory used to resolve PropertyEditors
    /// </summary>
    public interface IPropertyEditorFactory
    {
        Lazy<PropertyEditor, PropertyEditorMetadata> GetPropertyEditor(Guid id);
    }
}