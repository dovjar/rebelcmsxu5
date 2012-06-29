using Umbraco.Cms.Web.Context;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model;

namespace Umbraco.Cms.Web.PropertyEditors.NodeSelector
{
    /// <summary>
    /// The class that the dynamic start node query inherits from
    /// </summary>
    public abstract class AbstractStartNodeQuery
    {
        public abstract HiveId StartNodeId(IBackOfficeRequestContext requestContext, TypedEntity currentNode, TypedEntity rootNode);
    }
}