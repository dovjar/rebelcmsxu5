using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RebelCms.Cms.Web.Mvc.ActionFilters;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Model.BackOffice.UIElements
{
    [RebelCmsAuthorize(Permissions = new[] { FixedPermissionIds.Publish })]
    [UIElement("CC9CB1EB-75BE-49A4-938B-1332E4D43859", "RebelCms.UI.UIElements.ButtonUIElement")]
    public class PublishButtonUIElement : ButtonUIElement
    {
        public PublishButtonUIElement()
        {
            Alias = "Publish";
            Title = "Publish";
            CssClass = "publish-button toolbar-button";
            AdditionalData = new Dictionary<string, string>
            {
                { "id", "submit_Publish" },
                { "name", "submit.Publish" }
            };
        }
    }
}
