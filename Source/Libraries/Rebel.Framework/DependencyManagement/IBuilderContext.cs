using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Rebel.Framework.Configuration;

namespace Rebel.Framework.DependencyManagement
{
    public interface IBuilderContext
    {
        IConfigurationResolver ConfigurationResolver { get; }
        string MapPath(string relativePath);
    }
}
