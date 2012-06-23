using System.Collections.Generic;
using System.Collections.ObjectModel;
using RebelCms.Framework;
using RebelCms.Framework.Persistence.ProviderSupport._Revised;

namespace RebelCms.Hive.ProviderGrouping
{
    public class GroupedProviderMetadata : KeyedCollection<string, ProviderMetadata>
    {
        public GroupedProviderMetadata(IEnumerable<ProviderSupport.AbstractProviderRepository> childSessions)
        {
            childSessions.ForEach(x => Add(x.ProviderMetadata));
        }

        protected override string GetKeyForItem(ProviderMetadata item)
        {
            return item.Alias;
        }
    }
}