using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Framework;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Mvc.Controllers.BackOffice
{

    /// <summary>
    /// A controller to render a tree picker's markup either by a ChildAction of via a JSON request
    /// </summary>
    public class TreePickerController : BackOfficeController
    {
        private readonly IBackOfficeRequestContext _requestContext;

        public TreePickerController(IBackOfficeRequestContext requestContext)
            :base (requestContext)
        {
            _requestContext = requestContext;
        }

        /// <summary>
        /// Method that can be called by JavaScript to render a tree picker.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetPicker(TreePickerRenderModel model)
        {
            try
            {
                var htmlHelper = this.GetHtmlHelper(this.ControllerContext.RequestContext);
                var markup = htmlHelper.Action("Index", "TreePicker", new {model});
                return Json(new
                    {
                        Success = true,
                        Markup = markup.ToString()
                    });
            }
            catch (Exception ex)
            {
                return Json(new
                    {
                        Success = false,
                        Exception = ex.Message
                    });
            }
        }

        /// <summary>
        /// Renders a tree picker
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [ChildActionOnly]
        public ActionResult Index(TreePickerRenderModel model)
        {
            if (model != null && model.SelectedValue != null && !model.SelectedValue.Value.IsNullValueOrEmpty() && string.IsNullOrWhiteSpace(model.SelectedText))
            {
                model.SelectedText = "Unknown";

                var treeMetaData = _requestContext.RegisteredComponents
                    .TreeControllers
                    .Where(x => x.Metadata.Id == model.TreeControllerId)
                    .SingleOrDefault();

                //TreeVirtualRootId is obsolete, but if it is still used, we'll use the legacy way of checking for the node name.
                if (treeMetaData != null && model.SelectedValue.Value.Value == model.TreeVirtualRootId.Value)
                {
                    model.SelectedText = treeMetaData.Metadata.TreeTitle;
                }
                else
                {
                    var id = model.SelectedValue.Value;

                    if(id.ProviderGroupRoot == new Uri("security://"))
                    {
                        var member = _requestContext.Application.Security.Members.GetById(id);
                        model.SelectedText = member.Username;
                    }
                    else
                    {
                        using (var uow = _requestContext.Application.Hive.OpenReader<IContentStore>())
                        {
                            var entity = uow.Repositories.Get<TypedEntity>(model.SelectedValue.Value);
                            if (entity != null)
                            {
                                var nameAttr = entity.GetAttributeValueAsString(NodeNameAttributeDefinition.AliasValue, "Name");
                                // TODO: Can't guarantee attribute is "name"?)
                                if (!string.IsNullOrEmpty(nameAttr))
                                {
                                    model.SelectedText = Server.UrlEncode(nameAttr).Replace("+", "%20");
                                }
                                else if (model.SelectedValue.Value.IsSystem())
                                {
                                    //if the name is null, and the id is IsSystem, then we are going to happily assume that the 
                                    //node should be the root and will display the tree's metadata title.
                                    model.SelectedText = treeMetaData.Metadata.TreeTitle;
                                }
                            }
                        }
                    }
                }
            }

            return PartialView("TreePickerPartial", model);
        }

    }
}
