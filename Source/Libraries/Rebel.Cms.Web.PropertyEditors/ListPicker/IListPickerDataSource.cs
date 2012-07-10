using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rebel.Cms.Web.PropertyEditors.ListPicker
{
    public interface IListPickerDataSource
    {
        IDictionary<string, string> GetData();
    }
}
