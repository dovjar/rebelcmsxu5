using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Hive.ProviderGrouping;

namespace Rebel.Hive.RepositoryTypes
{
    [RepositoryType("media://")]
    public interface IMediaStore : IProviderTypeFilter
    { }
}
