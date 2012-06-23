using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using RebelCms.Cms;
using RebelCms.Cms.Web.Model.BackOffice.ParameterEditors;

namespace RebelCms.Cms.Web.DependencyManagement
{
    /// <summary>
    /// Used to resolve a ParameterEditor
    /// </summary>
    public class ParameterEditorFactory : IParameterEditorFactory
    {
        public Lazy<AbstractParameterEditor, ParameterEditorMetadata> GetParameterEditor(Guid id)
        {
            ////NOTE: Yes, we are using the resolver here, we need to be able to resolve new instances of property editors registered in the container
            var editors = DependencyResolver.Current.GetServices<Lazy<AbstractParameterEditor, ParameterEditorMetadata>>();

            var editorMeta = editors.GetParameterEditor(id);

            return editorMeta;
        }
    }
}
