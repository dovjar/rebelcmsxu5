using System;
using System.Collections.Generic;
using Rebel.Cms;
using Rebel.Cms.Web;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Rebel.Tests.Extensions
{
    public class MockedPropertyEditorFactory : IPropertyEditorFactory
    {
        public MockedPropertyEditorFactory(IRebelApplicationContext appContext)
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