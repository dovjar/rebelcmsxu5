using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Model.BackOffice.UIElements
{
    [RebelAuthorize(Permissions = new[] { FixedPermissionIds.Publish })]
    [UIElement("CC9CB1EB-75BE-49A4-938B-1332E4D43859", "Rebel.UI.UIElements.ButtonUIElement")]
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
