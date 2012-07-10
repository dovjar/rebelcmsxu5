using System;
using System.Linq;
using System.Web.Mvc;
using Rebel.Cms.Web.Context;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model;
using Rebel.Hive;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Cms.Web.Mvc
{
    /// <summary>
    /// Redirects to an Rebel page by Id or Entity
    /// </summary>
    public class RedirectToRebelPageResult : ActionResult
    {
        private TypedEntity _pageEntity;
        private readonly HiveId _pageId;
        private readonly IRoutableRequestContext _routableRequestContext;
        private string _url;
        public string Url
        {
            get
            {
                if (!_url.IsNullOrWhiteSpace()) return _url;

                if (PageEntity == null)
                {
                    throw new InvalidOperationException("Cannot redirect, no entity was found for id " + _pageId);
                }

                var result = _routableRequestContext.RoutingEngine.GetUrlForEntity(PageEntity);
                if (result.IsSuccess())
                {
                    _url = result.Url;
                    return _url;
                }
                
                throw new InvalidOperationException("Could not route to entity with id " + _pageId + ", the RoutingEngine could not generate a URL: " + result.Status);

            }
        }

        public TypedEntity PageEntity
        {
            get
            {
                if (_pageEntity != null) return _pageEntity;

                //need to get the URL for the page
                using (var uow = _routableRequestContext.Application.Hive.OpenWriter<IContentStore>())
                {
                    _pageEntity = uow.Repositories.Get<TypedEntity>(true, _pageId).SingleOrDefault();
                }

                return _pageEntity;
            }
        }

        /// <summary>
        /// Creates a new RedirectToRebelResult
        /// </summary>
        /// <param name="pageId"></param>
        public RedirectToRebelPageResult(HiveId pageId)
            : this(pageId, DependencyResolver.Current.GetService<IRoutableRequestContext>())
        {            
        }

        /// <summary>
        /// Creates a new RedirectToRebelResult
        /// </summary>
        /// <param name="pageEntity"></param>
        public RedirectToRebelPageResult(TypedEntity pageEntity)
            : this(pageEntity, DependencyResolver.Current.GetService<IRoutableRequestContext>())
        {
        }

        /// <summary>
        /// Creates a new RedirectToRebelResult
        /// </summary>
        /// <param name="pageEntity"></param>
        /// <param name="routableRequestContext"></param>
        public RedirectToRebelPageResult(TypedEntity pageEntity, IRoutableRequestContext routableRequestContext)
        {            
            _pageEntity = pageEntity;
            _pageId = pageEntity.Id;
            _routableRequestContext = routableRequestContext;
        }

        /// <summary>
        /// Creates a new RedirectToRebelResult
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="routableRequestContext"></param>
        public RedirectToRebelPageResult(HiveId pageId, IRoutableRequestContext routableRequestContext)
        {
            _pageId = pageId;
            _routableRequestContext = routableRequestContext;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            Mandate.ParameterNotNull(context, "context");
            if (context.IsChildAction)
            {
                throw new InvalidOperationException("Cannot redirect from a Child Action");
            }

            var destinationUrl = UrlHelper.GenerateContentUrl(Url, context.HttpContext);
            context.Controller.TempData.Keep();

            context.HttpContext.Response.Redirect(destinationUrl, endResponse: false);
        } 

    }
}