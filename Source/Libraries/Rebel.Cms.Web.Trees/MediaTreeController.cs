using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Editors;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Cms.Web.Security;
using Rebel.Cms.Web.Trees.MenuItems;
using Rebel.Framework;

using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Cms.Web;
using Rebel.Hive.ProviderGrouping;

namespace Rebel.Cms.Web.Trees
{
    [Tree(CorePluginConstants.MediaTreeControllerId, "Media")]
    [RebelTree]
    public class MediaTreeController : ContentTreeController
    {

        public MediaTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
        }

        public override Guid EditorControllerId
        {
            get { return new Guid(CorePluginConstants.MediaEditorControllerId); }
        }
        
        public override HiveId RecycleBinId
        {
            get { return FixedHiveIds.MediaRecylceBin; }
        }

        private HiveId _rootNodeId = HiveId.Empty;

        protected override HiveId RootNodeId
        {
            get
            {
                //if we've already resolved than dont attempt again
                if (_rootNodeId != HiveId.Empty)
                {
                    return _rootNodeId;
                }

                _rootNodeId = FixedHiveIds.MediaVirtualRoot;

                if (Request.IsAuthenticated && HttpContext.User.Identity is RebelBackOfficeIdentity)
                {
                    var currentIdentity = (RebelBackOfficeIdentity)HttpContext.User.Identity;
                    if(currentIdentity.StartMediaNode != HiveId.Empty)
                        _rootNodeId = currentIdentity.StartMediaNode;
                }

                return _rootNodeId;
            }
        }

        protected override HiveId ActualRootNodeId
        {
            get
            {
                return FixedHiveIds.MediaVirtualRoot;
            }
        }

        protected override ReadonlyGroupUnitFactory GetHiveProvider(HiveId parentId, FormCollection queryStrings)
        {
            //we need to get the Hive Map based on Id                        
            return BackOfficeRequestContext.Application.Hive.GetReader(
                parentId == FixedHiveIds.MediaVirtualRoot ? new Uri("media://") : parentId.ToUri());
        }

        protected override void AddMenuItemsToNode(TreeNode n, FormCollection queryStrings)
        {
            base.AddMenuItemsToNode(n, queryStrings);
            
            //TODO: now we just need to remove the ones we don't want from content
            n.MenuActions.RemoveAll(x =>
                                    x.Metadata.Id == BackOfficeRequestContext.RegisteredComponents.MenuItems.GetItem<Hostname>().Metadata.Id);
            n.MenuActions.RemoveAll(x =>
                                    x.Metadata.Id == BackOfficeRequestContext.RegisteredComponents.MenuItems.GetItem<Rollback>().Metadata.Id);

        }


    }
}
