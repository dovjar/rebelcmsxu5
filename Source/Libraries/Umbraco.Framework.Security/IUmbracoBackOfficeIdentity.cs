using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Framework.Security
{
    public interface IUmbracoBackOfficeIdentity : IUmbracoIdentity
    {
        HiveId StartContentNode { get; }
        HiveId StartMediaNode { get;  }
        string[] AllowedApplications { get; }
    }
}
