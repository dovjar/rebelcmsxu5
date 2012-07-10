using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rebel.Framework.Security
{
    public interface IRebelIdentity
    {
        HiveId Id { get; }
        string Name { get; }
        string[] Roles { get; }
    }
}
