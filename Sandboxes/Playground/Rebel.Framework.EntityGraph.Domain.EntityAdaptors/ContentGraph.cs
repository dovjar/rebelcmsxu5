using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RebelFramework.EntityGraph.Domain.Abstracts;

namespace RebelFramework.EntityGraph.Synonyms
{
    public class ContentGraph : EntitySynonymGraph<IContentEntitySynonym>
    {
        public ContentGraph(IList<Content> incoming)
            : base(incoming.Cast<IContentEntitySynonym>().ToList())
        {

        }

    }
}
