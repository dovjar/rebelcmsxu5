using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Framework
{
    public class ObsoleteException : InvalidOperationException
    {
        public ObsoleteException(string obsolete)
            : base(string.Format("'{0}' is obsolete.", obsolete))
        { }

        public ObsoleteException(string obsolete, string replacement)
            : base(string.Format("'{0}' is obsolete. Please use '{1}' instead.", obsolete, replacement))
        { }
    }
}
