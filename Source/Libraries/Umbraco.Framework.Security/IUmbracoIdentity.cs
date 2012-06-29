using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Framework.Security
{
    public interface IUmbracoIdentity
    {
        HiveId Id { get; }
        string Name { get; }
        string[] Roles { get; }
    }
}
