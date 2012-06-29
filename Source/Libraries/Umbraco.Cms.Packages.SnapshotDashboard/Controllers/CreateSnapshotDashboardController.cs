using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using System.IO;
using System.Linq;
using Umbraco.Cms.Packages.SnapshotDashboard.Models;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Editors;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Serialization;
using Umbraco.Hive;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Packages.SnapshotDashboard.Controllers
{
    [Editor("692AF9E8-4F85-4668-BF5D-3FEE412C5F72", HasChildActionDashboards = true)]
    public class CreateSnapshotDashboardController : DashboardEditorController
    {
        private readonly IBackOfficeRequestContext _requestContext;
        private readonly IGroupUnitFactory<IFileStore> _hive;

        public CreateSnapshotDashboardController(IBackOfficeRequestContext requestContext) : base(requestContext)
        {
            _requestContext = requestContext;
            _hive = requestContext.Application.Hive.GetWriter<IFileStore>(new Uri("storage://snapshots"));
        }

        [ChildActionOnly]
        public PartialViewResult DisplaySnapshotForm()
        {
            var snapshotModel = new SnapshotModel();

            //Return view with our model
            return PartialView("SnapshotDashboard", snapshotModel);
        }

        [HttpPost]
        public JsonResult CreateSnapshot(SnapshotModel model)
        {
            var resultModel = new SnapshotResultModel();
            var snap = DateTime.Now.Ticks.ToString();
            resultModel.SnapshotCreated = true;
            resultModel.SnapshotLocation = "/App_Data/Data/" + snap;

            using (var uow = _hive.Create())
            {
                // Create folder
                var folder = new Umbraco.Framework.Persistence.Model.IO.File(snap, "") { IsContainer = true };
                uow.Repositories.AddOrUpdate(folder);
                uow.Complete();
            }

            var docTypeList = new Dictionary<string, string>();
            var contentList = new Dictionary<string, string>();
            var mediaList = new Dictionary<string, string>();

            if (!model.IncludeDocumentTypes)
            {
                //Access ContentStore and get all distinct document types
                using (var uow = _requestContext.Application.Hive.OpenReader<IContentStore>())
                {
                    var docTypes =
                        uow.Repositories.Schemas.GetDescendentRelations(FixedHiveIds.ContentRootSchema,
                                                                        FixedRelationTypes.DefaultRelationType)
                            .Where(x => !x.DestinationId.IsSystem())
                            .DistinctBy(x => x.DestinationId);

                    foreach (var docType in docTypes)
                    {
                        var schema = uow.Repositories.Schemas.Get<EntitySchema>(docType.DestinationId);
                        var result = _requestContext.Application.FrameworkContext.Serialization.ToStream(schema);
                        docTypeList.Add(docType.DestinationId.Value.ToString(), result.ResultStream.ToJsonString());
                    }
                }

                //Dump json strings as files to 'DocumentType' data folder
                if(docTypeList.Any())
                {
                    using (var uow = _hive.Create())
                    {
                        foreach (var pair in docTypeList)
                        {
                            var file = new Umbraco.Framework.Persistence.Model.IO.File(
                                snap + "/DocumentTypes/" + pair.Key,
                                Encoding.UTF8.GetBytes(pair.Value));
                            uow.Repositories.AddOrUpdate(file);
                        }

                        uow.Complete();
                    }
                }
            }

            if (!model.IncludeMedia)
            {
                //Access ContentStore and get all distinct media
                using (var uow = _requestContext.Application.Hive.OpenReader<IMediaStore>())
                {
                    var medias =
                        uow.Repositories.Schemas.GetDescendentRelations(FixedHiveIds.MediaVirtualRoot,
                                                                        FixedRelationTypes.DefaultRelationType)
                            .Where(x => !x.DestinationId.IsSystem())
                            .DistinctBy(x => x.DestinationId);

                    foreach (var media in medias)
                    {
                        var schema = uow.Repositories.Get(media.DestinationId);
                        var result = _requestContext.Application.FrameworkContext.Serialization.ToStream(schema);
                        mediaList.Add(media.DestinationId.Value.ToString(), result.ResultStream.ToJsonString());
                    }
                }

                //Dump json strings as files to 'Media' data folder
                if(mediaList.Any())
                {
                    using (var uow = _hive.Create())
                    {
                        foreach (var pair in docTypeList)
                        {
                            var file = new Umbraco.Framework.Persistence.Model.IO.File(
                                snap + "/Media/" + pair.Key,
                                Encoding.UTF8.GetBytes(pair.Value));
                            uow.Repositories.AddOrUpdate(file);
                        }

                        uow.Complete();
                    }
                }
            }

            if (!model.IncludeContent)
            {
                //Access ContentStore and get all distinct content - latest revision
                using (var uow = _requestContext.Application.Hive.OpenReader<IContentStore>())
                {
                    var contents =
                        uow.Repositories.Schemas.GetDescendentRelations(FixedHiveIds.ContentVirtualRoot,
                                                                        FixedRelationTypes.DefaultRelationType)
                            .Where(x => !x.DestinationId.IsSystem())
                            .DistinctBy(x => x.DestinationId);

                    foreach (var content in contents)
                    {
                        var schema = uow.Repositories.Get(content.DestinationId);
                        var result = _requestContext.Application.FrameworkContext.Serialization.ToStream(schema);
                        contentList.Add(content.DestinationId.Value.ToString(), result.ResultStream.ToJsonString());
                    }
                }

                //Dump json strings as files to 'Content' data folder
                if(contentList.Any())
                {
                    using (var uow = _hive.Create())
                    {
                        foreach (var pair in docTypeList)
                        {
                            var file = new Umbraco.Framework.Persistence.Model.IO.File(
                                snap + "/Content/" + pair.Key,
                                Encoding.UTF8.GetBytes(pair.Value));
                            uow.Repositories.AddOrUpdate(file);
                        }

                        uow.Complete();
                    }
                }
            }

            //Set Success
            resultModel.NotificationTitle = "Snapshot created";
            resultModel.NotificationMessage = "A snapshot with the selected types has been created";
            resultModel.NotificationType = NotificationType.Success.ToString().ToLower();

            //Invalid data in model (client slide validation should catch this, in as fail safe)
            /*resultModel.SnapshotCreated = false;

            resultModel.NotificationTitle = "An error occured";
            resultModel.NotificationMessage = "Some of the data was invalid";
            resultModel.NotificationType = NotificationType.Error.ToString().ToLower();*/

            //Return some JSON
            return Json(resultModel);
        }
    }
}