using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.Model.BackOffice.ParameterEditors;

namespace Umbraco.Cms.Web.ParameterEditors.MediaTypePicker
{
    [ParameterEditor("F5367557-CAE6-4591-AF3B-DA30562DF7D9", "MediaTypePicker", "Media Type Picker", CorePluginConstants.TypePickerPropertyEditorId)]
    public class MediaTypePicker : ParameterEditor
    {
        public MediaTypePicker(IPropertyEditorFactory propertyEditorFactory) 
            : base(propertyEditorFactory)
        { }

        public override string PropertyEditorPreValues
        {
            get
            {
                return @"
                    <preValues>
                        <preValue name='Type'><![CDATA[Media]]></preValue>
                    </preValues>";
            }
        }
    }
}
