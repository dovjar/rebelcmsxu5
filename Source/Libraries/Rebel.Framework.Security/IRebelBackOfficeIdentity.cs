using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rebel.Framework.Security
{
    public interface IRebelBackOfficeIdentity : IRebelIdentity
    {
        HiveId StartContentNode { get; }
        HiveId StartMediaNode { get;  }
        string[] AllowedApplications { get; }
    }
}
