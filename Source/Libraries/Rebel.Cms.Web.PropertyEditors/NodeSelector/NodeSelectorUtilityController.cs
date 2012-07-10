using System;
using System.Linq;
using System.Web.Compilation;
using System.Web.Mvc;
using Rebel.Cms.Web.BuildManagerCodeDelegates;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model.BackOffice.Editors;
using Rebel.Cms.Web.Mvc;
using Rebel.Cms.Web.Mvc.Controllers.BackOffice;
using Rebel.Framework;
using Rebel.Framework.Diagnostics;
using Rebel.Framework.Localization;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence;
using Rebel.Framework.Persistence.Model.Constants.AttributeDefinitions;

namespace Rebel.Cms.Web.PropertyEditors.NodeSelector
{
    /// <summary>
    /// A utility controller for the NodeSelector
    /// </summary>
    public class NodeSelectorUtilityController : SecuredBackOfficeController
    {
        public NodeSelectorUtilityController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
        }

        /// <summary>
        /// Checks if the node is selectable when clicked based on the filter applied on the prevalue editor
        /// </summary>
        /// <param name="dataTypeId"></param>
        /// <param name="nodeId"></param>
        /// <param name="treeId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult IsNodeSelectable(HiveId dataTypeId, HiveId nodeId, Guid treeId)
        {
            var ds = BackOfficeRequestContext.RegisteredComponents.TreeControllers.GetNodeSelectorDataSource(treeId);
            
            try
            {
                return Json(new
                {
                    result = CSharpExpressionsUtility.GetNodeFilterResult(BackOfficeRequestContext, dataTypeId, ds, nodeId),
                    success = true
                });
            }
            catch (Exception ex)
            {
                var errorMsg = "NodeSelector filter error (see rebel log for full details): " + ex.Message;
                LogHelper.Error<NodeSelectorUtilityController>("The filter executed on the node caused an error", ex);
                Notifications.Add(new NotificationMessage(errorMsg, "Compilation error", NotificationType.Error));
                return new CustomJsonResult(new
                {
                    success = false,
                    notifications = Notifications,
                    errorMsg
                }.ToJsonString);                
            }              
            
        }

        /// <summary>
        /// Returns the media url for a media item given the entity id and the attribute alias, this proxies the request to the
        /// current NodeSelector data source
        /// </summary>
        /// <param name="id"></param>
        /// <param name="treeId"></param>
        /// <param name="attributeAlias"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetMediaUrl(HiveId id, Guid treeId, string attributeAlias)
        {
            try
            {
                var ds = BackOfficeRequestContext.RegisteredComponents.TreeControllers.GetNodeSelectorDataSource(treeId);
                return Json(new
                    {
                        url = ds.GetMediaUrl(id, treeId, attributeAlias)
                    });
            }
            catch (Exception)
            {
                return Json(new
                    {
                        url = ""
                    });
            }
        }

        /// <summary>
        /// returns the tooltip contents for the entity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="treeId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetTooltipContent(HiveId id, Guid treeId)
        {
            var ds = BackOfficeRequestContext.RegisteredComponents.TreeControllers.GetNodeSelectorDataSource(treeId);
            var tooltip = ds.GetTooltipContents(id, treeId);
            return Json(new
                {
                    content = "<div class='tooltipInfo'>" + tooltip.HtmlContent + "</div>",
                    height = tooltip.Height,
                    width = tooltip.Width
                });            
        }

        /// <summary>
        /// Returns the EntityPaths for the specified HiveId from the treeId specified which must be NodeSelectorCompatible.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="treeId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetPath(HiveId id, Guid treeId)
        {
            var ds = BackOfficeRequestContext.RegisteredComponents.TreeControllers.GetNodeSelectorDataSource(treeId);
            return new CustomJsonResult(() => ds.GetPaths(id, treeId).ToJson().ToString());
        }
    }
}