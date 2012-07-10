using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Foundation;
using Rebel.Framework.Persistence.Model.Versioning;

namespace Rebel.Framework.Persistence.Model.Constants
{
    public class DraftStatus : RevisionStatusType
    {
        public DraftStatus()
        {
            Id = 0;
            Alias = "draft";
            Name = "Draft";
            IsSystem = true;
        }

        public static DraftStatus Default { get { return new DraftStatus(); } }
    }
}
