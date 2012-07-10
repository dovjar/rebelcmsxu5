using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Cms.Web.Mvc.ActionInvokers;
using Rebel.Framework;
using Rebel.Framework.Persistence;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.AttributeTypes;
using Rebel.Framework.Persistence.Model.IO;
using Rebel.Framework.Security;
using Rebel.Hive;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Cms.Web.Mvc.Controllers
{
    /// <summary>
    /// Controller to return and secure Media files
    /// </summary>
    [InstalledFilter]
    public class MediaProxyController : Controller, IRequiresRoutableRequestContext
    {
        private readonly static Regex SizePattern = new Regex(@".*_([0-9]+)\.", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public IRoutableRequestContext RoutableRequestContext { get; set; }

        public MediaProxyController()
            : this(DependencyResolver.Current.GetService<IRoutableRequestContext>())
        { }

        public MediaProxyController(IRoutableRequestContext routableRequestContext)
        {
            ActionInvoker = new RoutableRequestActionInvoker(RoutableRequestContext);
            RoutableRequestContext = routableRequestContext;
        }

        /// <summary>
        /// Displays the image, ensuring the correct permissions are set.
        /// </summary>
        /// <param name="propertyAlias"></param>
        /// <param name="mediaId"></param>
        /// <param name="size"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public virtual ActionResult Proxy(string propertyAlias, string mediaId, int size, string fileName)
        {
            if (mediaId.IsNullOrWhiteSpace())
                return HttpNotFound();

            var app = RoutableRequestContext.Application;
            using (var uow = app.Hive.OpenReader<IContentStore>())
            {
                // Lookup a TypedEntity with an Upload field that has a MediaId of the mediaId property
                var entity =
                    uow.Repositories.SingleOrDefault(
                        x => x.InnerAttribute<string>(propertyAlias, "MediaId") == mediaId);

                if (entity == null)
                    return HttpNotFound();

                var member = app.Security.Members.GetCurrent();
                if (member == null || !app.Security.PublicAccess.GetPublicAccessStatus(member.Id, entity.Id).CanAccess)
                {
                    // Member can't access,  but check to see if logged in identiy is a User, if so, just allow access
                    var ticket = HttpContext.GetRebelAuthTicket();
                    if (ticket == null || ticket.Expired)
                    {
                        return HttpNotFound();
                    }
                }

                // Find the upload property
                var property = entity.Attributes.SingleOrDefault(x => x.AttributeDefinition.AttributeType.RenderTypeProvider.Equals(CorePluginConstants.FileUploadPropertyEditorId, StringComparison.InvariantCultureIgnoreCase) && x.Values["MediaId"].ToString() == mediaId);

                if (property == null)
                    return HttpNotFound();

                // Get the file
                var fileId = new HiveId(property.DynamicValue);
                using (var uow2 = app.Hive.OpenReader<IFileStore>(fileId.ToUri()))
                {
                    var file = uow2.Repositories.Get<File>(fileId);

                    if (size > 0)
                    {
                        // Look for thubnail file
                        var relation = uow2.Repositories.GetLazyChildRelations(fileId, FixedRelationTypes.ThumbnailRelationType)
                            .SingleOrDefault(x => x.MetaData.Single(y => y.Key == "size").Value == size.ToString());

                        if (relation != null && relation.Destination != null)
                        {
                            var thumbnail = (File)relation.Destination;
                            return File(thumbnail.ContentBytes, thumbnail.GetMimeType());
                        }

                        return HttpNotFound();
                    }

                    if (file != null)
                        return File(file.ContentBytes, file.GetMimeType());
                }
            }

            return HttpNotFound();
        }
    }
}
