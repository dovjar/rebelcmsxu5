using System;
using System.Collections.Generic;
using System.Linq;
using Rebel.Cms.Web;
using Rebel.Cms.Web.Routing;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Versioning;

namespace Rebel.Tests.Extensions
{
    using Rebel.Cms.Web.Context;

    public class FakeRoutingEngine : IRoutingEngine
    {
        public ReadonlyHostnameCollection DomainList
        {
            get { return new ReadonlyHostnameCollection(new HostnameCollection()); }
        }

        public IRebelApplicationContext ApplicationContext
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void ClearCache(bool clearDomains = false, HiveId[] clearForIds = null, bool clearMappedUrls = false, bool clearGeneratedUrls = false, bool clearAll = false)
        {
            
        }

        public EntityRouteResult FindEntityByUrl(Uri fullUrlIncludingDomain, RevisionStatusType revisionStatusType = null)
        {
            return null;
        }

        public UrlResolutionResult GetUrlForEntity(TypedEntity entity)
        {
            return new UrlResolutionResult("/this-is-a-test", UrlResolutionStatus.SuccessWithoutHostname);
        }

        public UrlResolutionResult[] GetAllUrlsForEntity(TypedEntity entity)
        {
            return new UrlResolutionResult[0];
        }

    }
}