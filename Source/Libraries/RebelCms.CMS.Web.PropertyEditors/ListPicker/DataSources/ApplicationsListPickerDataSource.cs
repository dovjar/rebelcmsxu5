using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RebelCms.Cms.Web.Configuration;

namespace RebelCms.Cms.Web.PropertyEditors.ListPicker.DataSources
{
    public class ApplicationsListPickerDataSource : IListPickerDataSource
    {
        public IDictionary<string, string> GetData()
        {
            return RebelCmsSettings.GetSettings().Applications.ToDictionary(x => x.Alias, y => y.Name);
        }
    }
}
