using System;
using Rebel.Cms.Web.Model.BackOffice.ParameterEditors;

namespace Rebel.Cms.Web
{
    /// <summary>
    /// A Factory used to resolve ParameterEditors
    /// </summary>
    public interface IParameterEditorFactory
    {
        Lazy<AbstractParameterEditor, ParameterEditorMetadata> GetParameterEditor(Guid id);
    }
}
