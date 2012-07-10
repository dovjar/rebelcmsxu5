using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Cms.Web.Configuration;

namespace Rebel.Cms.Web.PropertyEditors.ListPicker.DataSources
{
    public class ApplicationsListPickerDataSource : IListPickerDataSource
    {
        public IDictionary<string, string> GetData()
        {
            return RebelSettings.GetSettings().Applications.ToDictionary(x => x.Alias, y => y.Name);
        }
    }
}
