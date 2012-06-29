using Umbraco.Framework.Security;
using Umbraco.Framework.Security.Model.Entities;

namespace Umbraco.Cms.Web.FluentExtensions
{
    using Umbraco.Hive;
    using Umbraco.Hive.RepositoryTypes;

    public interface ICmsBuilderStep<out TProviderFilter> : IBuilderStep<TProviderFilter>
        where TProviderFilter : class, IProviderTypeFilter
    {
        
    }
}