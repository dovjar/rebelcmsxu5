using System.Collections.Generic;

namespace RebelCmsFramework
{
    public interface IQueryBroker
    {
        IQueryBroker WithId(IMappedIdentifier identifier);
        IQueryBroker WithId(IEnumerable<IMappedIdentifier> identifiers);
        IQueryBroker FromProvider(IProviderManifest provider);
        IQueryBroker ToDepth(int traversalDepth);
    }
}