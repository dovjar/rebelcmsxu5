using Rebel.Cms;
using Rebel.Cms.Web;
using Rebel.Cms.Web.Model.BackOffice.ParameterEditors;

namespace Rebel.Cms.Web.ParameterEditors.Integer
{
    [ParameterEditor("718FC645-920E-4216-BD81-A99CDFC3B74C", "Integer", "Integer", CorePluginConstants.NumericPropertyEditorId)]
    public class IntegerEditor : ParameterEditor
    {
        public IntegerEditor(IPropertyEditorFactory propertyEditorFactory)
            : base(propertyEditorFactory)
        { }

        public override string PropertyEditorPreValues
        {
            get
            {
                return @"
                    <preValues>
                        <preValue name='DecimalPlaces'><![CDATA[0]]></preValue>
                    </preValues>";
            }
        }
    }
}
