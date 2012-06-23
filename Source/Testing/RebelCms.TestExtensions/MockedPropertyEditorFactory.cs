using System;
using System.Collections.Generic;
using RebelCms.Cms;
using RebelCms.Cms.Web;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Model.BackOffice.PropertyEditors;

namespace RebelCms.Tests.Extensions
{
    public class MockedPropertyEditorFactory : IPropertyEditorFactory
    {
        public MockedPropertyEditorFactory(IRebelCmsApplicationContext appContext)
        {
            _propEditors = new FixedPropertyEditors(appContext).GetPropertyEditorDefinitions();
        }

        private readonly IEnumerable<Lazy<PropertyEditor, PropertyEditorMetadata>>  _propEditors;

        public Lazy<PropertyEditor, PropertyEditorMetadata> GetPropertyEditor(Guid id)
        {
            return _propEditors.GetPropertyEditor(id);
        }
    }
}