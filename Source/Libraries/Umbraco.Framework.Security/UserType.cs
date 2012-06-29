using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Framework.Security
{
    [Flags]
    public enum UserType
    {
        None,
        User,
        Member
    }
}
