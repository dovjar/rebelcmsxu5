using RebelCms.Framework.EntityGraph.Domain;
using RebelCms.Framework.EntityGraph.Domain.Brokers;
using RebelCms.Framework.EntityGraph.Domain.EntityAdaptors;

namespace Prototyping.RebelCms.Framework.Playground
{
    public class Article : Content
    {
        public Article()
        {
            // Basic setup
            this.Setup();

            // Get the DocType for this Content
            EntityType = new ArticleDocType();

            this.AsDynamic().textdata = "New text data";
        }
    }
}