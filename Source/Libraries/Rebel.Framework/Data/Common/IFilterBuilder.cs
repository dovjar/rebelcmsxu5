using System.Collections.Generic;

namespace Rebel.Framework.Data.Common
{
    public interface IFilterBuilder
    {
        IFilterBuilder WithId(IMappedIdentifier identifier);
        IFilterBuilder WithId(IEnumerable<IMappedIdentifier> identifiers);
        IFilterBuilder FromProvider(IProviderManifest provider);
        IFilterBuilder ToDepth(int traversalDepth);
    }
}