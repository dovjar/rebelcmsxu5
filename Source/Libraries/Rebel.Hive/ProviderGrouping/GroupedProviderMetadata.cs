using System.Collections.Generic;
using System.Collections.ObjectModel;
using Rebel.Framework;
using Rebel.Framework.Persistence.ProviderSupport._Revised;

namespace Rebel.Hive.ProviderGrouping
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