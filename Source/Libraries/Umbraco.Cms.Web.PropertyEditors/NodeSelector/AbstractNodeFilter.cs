using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Framework.Persistence.Model;

namespace Umbraco.Cms.Web.PropertyEditors.NodeSelector
{
    /// <summary>
    /// The class that the dynamic filter inherits from
    /// </summary>
    public abstract class AbstractNodeFilter
    {
        public abstract bool IsMatch(IBackOfficeRequestContext requestContext, TypedEntity entity);
    }
}