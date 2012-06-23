using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RebelCms.Hive.ProviderGrouping;

namespace RebelCms.Hive.RepositoryTypes
{
    [RepositoryType("media://")]
    public interface IMediaStore : IProviderTypeFilter
    { }
}
