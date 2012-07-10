using System;
using System.Linq;
using System.Web.Mvc;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Editors.Extenders;
using Rebel.Cms.Web.Model.BackOffice.Editors;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Cms.Web.Mvc.ActionInvokers;
using Rebel.Cms.Web.Mvc.Controllers;
using Rebel.Cms.Web.Mvc.ViewEngines;

using Rebel.Framework;
using Rebel.Framework.Localization;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Rebel.Framework.Persistence.Model.Versioning;
using Rebel.Hive;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Cms.Web.Editors
{
    //[Editor(CorePluginConstants.MediaEditorControllerId)]
    //[RebelEditor]
    //public class MemberEditorController : AbstractContentEditorController
    //{

    //}

    [Editor(CorePluginConstants.MediaEditorControllerId)]
    [RebelEditor]        
    [AlternateViewEnginePath("ContentEditor")]
    [ExtendedBy(typeof(MoveCopyController), AdditionalParameters = new object[] { CorePluginConstants.MediaTreeControllerId })]
    [ExtendedBy(typeof(PublicAccessController))]
    public class MediaEditorController : AbstractRevisionalContentEditorController<MediaEditorModel>
    {

        public MediaEditorController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
            _hive = BackOfficeRequestContext.Application.Hive.GetWriter(new Uri("media://"));
            _readonlyHive = BackOfficeRequestContext.Application.Hive.GetReader<IContentStore>();

            Mandate.That(_hive != null, x => new NullReferenceException("Could not find hive provider for route media://"));
        }

        private readonly GroupUnitFactory _hive;
        private readonly ReadonlyGroupUnitFactory _readonlyHive;

        /// <summary>
        /// Returns the hive provider used for this controller
        /// </summary>
        public override GroupUnitFactory Hive
        {
            get { return _hive; }
        }

        public override ReadonlyGroupUnitFactory ReadonlyHive
        {
            get
            {
                return _readonlyHive;
            }
        }

        /// <summary>
        /// Returns the recycle bin id used for this controller
        /// </summary>
        public override HiveId RecycleBinId
        {
            get { return FixedHiveIds.MediaRecylceBin; }
        }

        /// <summary>
        /// Return the media root as the virtual root node
        /// </summary>
        public override HiveId VirtualRootNodeId
        {
            get { return FixedHiveIds.MediaVirtualRoot; }
        }

        /// <summary>
        /// Returns the media virtual root
        /// </summary>
        public override HiveId RootSchemaNodeId
        {
            get { return FixedHiveIds.MediaRootSchema; }
        }

        public override string CreateNewTitle
        {
            get { return "Create new media"; }
        }

        protected override void OnEditing(MediaEditorModel model, EntitySnapshot<TypedEntity> entity)
        {
            //we need to flag if this model is editable based on whether or not it is in the recycle bin
            using (var uow = Hive.Create<IContentStore>())
            {
                var ancestorIds = uow.Repositories.GetAncestorIds(entity.Revision.Item.Id, FixedRelationTypes.DefaultRelationType);
                if (ancestorIds.Contains(FixedHiveIds.MediaRecylceBin, new HiveIdComparer(true)))
                {
                    model.IsEditable = false;
                }
            }
        }

    }

}
