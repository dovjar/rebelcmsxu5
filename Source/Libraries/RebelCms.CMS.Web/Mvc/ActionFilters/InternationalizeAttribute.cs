using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Mvc;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Model;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Hive;
using RebelCms.Hive.RepositoryTypes;

namespace RebelCms.Cms.Web.Mvc.ActionFilters
{
    public class InternationalizeAttribute : ActionFilterAttribute
    {
        private IRebelCmsApplicationContext _applicationContext;
        public IRebelCmsApplicationContext ApplicationContext
        {
            get { return _applicationContext ?? (_applicationContext = DependencyResolver.Current.GetService<IRebelCmsApplicationContext>()); }
            set { _applicationContext = value; }
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var model = filterContext.ActionParameters.SingleOrDefault(x => x.Key == "model").Value as IRebelCmsRenderModel;
            if (model == null)
                return;

            var currentNodeId = model.CurrentNode.Id;

            //TODO: Cache this

            using (var uow = ApplicationContext.Hive.OpenReader<IContentStore>())
            {
                var ancestorIds = uow.Repositories.GetAncestorsIdsOrSelf(currentNodeId, FixedRelationTypes.DefaultRelationType);
                foreach (var ancestorId in ancestorIds)
                {
                    var languageRelation =
                        uow.Repositories.GetParentRelations(ancestorId, FixedRelationTypes.LanguageRelationType).
                            SingleOrDefault();

                    if (languageRelation == null) 
                        continue;

                    var isoCode = languageRelation.MetaData.SingleOrDefault(x => x.Key == "IsoCode").Value;

                    Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(isoCode);
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(isoCode);

                    return;
                }
            }
        }
    }
}
