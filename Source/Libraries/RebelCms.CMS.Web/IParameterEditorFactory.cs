using System;
using RebelCms.Cms.Web.Model.BackOffice.ParameterEditors;

namespace RebelCms.Cms.Web
{
    /// <summary>
    /// A Factory used to resolve ParameterEditors
    /// </summary>
    public interface IParameterEditorFactory
    {
        Lazy<AbstractParameterEditor, ParameterEditorMetadata> GetParameterEditor(Guid id);
    }
}
