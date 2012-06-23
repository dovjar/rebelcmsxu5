using System.Collections.Generic;
using RebelCmsFramework.Models.EntityHub;

namespace RebelCmsFramework.Concepts.EntityHub
{
    public interface IEntityResolver
    {
        IComplexEntity Get(IEntityIdentifier identifier);
        IEnumerable<IComplexEntity> Get(IEnumerable<IEntityIdentifier> identifiers);
        IEnumerable<IComplexEntity> GetAll();

        IComplexEntity Get(IEntityIdentifier identifier, int traversalDepth);
        IEnumerable<IComplexEntity> Get(IEnumerable<IEntityIdentifier> identifiers, int traversalDepth);
        IEnumerable<IComplexEntity> GetAll(int traversalDepth);

        
    }
}
