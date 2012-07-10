using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RebelFramework;
using RebelFramework.EntityGraph.DataPersistence;
using RebelFramework.EntityGraph.Domain;
using RebelFramework.EntityGraph.Domain.Entity.Graph;
using RebelFramework.EntityGraph.Domain.ObjectModel;
using RebelFramework.EntityGraph.Synonyms;

namespace Prototyping.RebelFramework
{
    public class MockContentBroker : IContentResolver
    {
        #region Implementation of IContentResolver

        public IEntitySynonymVertex<IContentVertex> Get(IMappedIdentifier identifier)
        {
            throw new NotImplementedException();
        }

        public EntitySynonymGraph<IContentVertex> Get(IEnumerable<IMappedIdentifier> identifiers)
        {
            throw new NotImplementedException();
        }

        public EntitySynonymGraph<IContentVertex> GetAll()
        {
            throw new NotImplementedException();
        }

        public IEntitySynonymVertex<IContentVertex> Get(IMappedIdentifier identifier, int traversalDepth)
        {
            throw new NotImplementedException();
        }

        public EntitySynonymGraph<IContentVertex> Get(IEnumerable<IMappedIdentifier> identifiers, int traversalDepth)
        {
            throw new NotImplementedException();
        }

        public EntitySynonymGraph<IContentVertex> GetAll(int traversalDepth)
        {
            throw new NotImplementedException();
        }

        public IEntityRepositoryReader RepositoryReader { get; set; }
        public IEntityRepositoryWriter RepositoryWriter { get; set; }

        #endregion
    }
}
