using System;
using System.Collections.Generic;
using System.Linq;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model.BackOffice.PropertyEditors;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Cms.Web.PropertyEditors.DateTimePicker;
using Rebel.Cms.Web.PropertyEditors.Label;
using Rebel.Cms.Web.PropertyEditors.ListPicker;
using Rebel.Cms.Web.PropertyEditors.Numeric;
using Rebel.Cms.Web.PropertyEditors.TreeNodePicker;
using Rebel.Cms.Web.PropertyEditors.TrueFalse;
using Rebel.Cms.Web.PropertyEditors.RebelSystemEditors.NodeName;
using Rebel.Cms.Web.PropertyEditors.RebelSystemEditors.SelectedTemplate;
using Rebel.Cms.Web.PropertyEditors.Upload;
using Rebel.Cms.Web.Trees;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Tests.Extensions.Stubs.PropertyEditors;
using Rebel.Cms.Web;

namespace Rebel.Tests.Extensions
{
    public class FixedPropertyEditors
    {
        public FixedPropertyEditors(IRebelApplicationContext fakeRebelApplicationContext)
        {
            var fakeBackOfficeContext = new FakeBackOfficeRequestContext(fakeRebelApplicationContext);

            _propertyEditors = new PropertyEditor[]
                {
                    new NonMandatoryPropertyEditor(Guid.Parse("D3DC1AC8-F83D-4D73-A13B-024E3100A600"), "rte", ""),
                    new NonMandatoryPropertyEditor(Guid.Parse("781FA5A4-C26D-4BBB-9F58-6CBBE0A9D14A"), "cust", ""),
                    new NonMandatoryPropertyEditor(Guid.Parse("3F5ED845-7018-4BDE-AB4E-C7106EE0992D"), "text", ""),
                    new NonMandatoryPropertyEditor(Guid.Parse("9CCB7060-3550-11E0-8A80-074CDFD72085"), "swatch", ""),
                    new NonMandatoryPropertyEditor(Guid.Parse("2B4DF3F1-C84E-4611-87EE-1D90ED437337"), "tags", ""),
                    new NonMandatoryPropertyEditor(),
                    new MandatoryPropertyEditor(),
                    new RegexPropertyEditor(),
                    new PreValueRegexPropertyEditor(),
                    //ensure node name is there
                    new NodeNameEditor(fakeBackOfficeContext.Application),
                    new SelectedTemplateEditor(fakeBackOfficeContext),
                    new UploadEditor(fakeBackOfficeContext),
                    new LabelEditor(),
                    new TrueFalseEditor(),
                    new DateTimePickerEditor(),
                    new NumericEditor(),
                    new TreeNodePicker(Enumerable.Empty<Lazy<TreeController, TreeMetadata>>()),
                    new ListPickerEditor(),
                    new TestContentAwarePropertyEditor()
                };
        }

        private readonly PropertyEditor[] _propertyEditors;

        public IEnumerable<Lazy<PropertyEditor, PropertyEditorMetadata>> GetPropertyEditorDefinitions()
        {
            return _propertyEditors.ToPropertyEditorDefinitions();
        }

        public IEnumerable<PropertyEditor> GetPropertyEditors()
        {
            return _propertyEditors;
        }
    }
}