using System;

using Rebel.Framework.DataManagement;
using Rebel.Framework.Persistence.DataManagement;

namespace Rebel.Framework.IO
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