using System.Web.Mvc;
using RebelCms.Framework.Localization;
using RebelCms.Framework.Localization.Web.Mvc;

namespace RebelCms.Framework
{
    public static class ControllerContextExtensions
    {
        public static TextManager TextManager(this ControllerContext ctx)
        {
            return LocalizationHelper.TextManager;
        }
    }
}
