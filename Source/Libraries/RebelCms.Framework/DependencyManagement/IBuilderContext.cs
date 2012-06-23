using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RebelCms.Framework.Configuration;

namespace RebelCms.Framework.DependencyManagement
{
    public interface IBuilderContext
    {
        IConfigurationResolver ConfigurationResolver { get; }
    }
}
