using System;

using RebelCms.Framework.DataManagement;
using RebelCms.Framework.Persistence.DataManagement;

namespace RebelCms.Framework.IO
{
    public class Transaction : AbstractTransaction
    {
        protected override bool PerformExplicitRollback()
        {
            return true;
        }

        protected override bool PerformImplicitRollback()
        {
            return true;
        }

        protected override bool PerformCommit()
        {
            return true;
        }
    }
}