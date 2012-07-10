using System.Web.Mvc;
using Rebel.Framework.Localization;
using Rebel.Framework.Localization.Web.Mvc;

namespace Rebel.Framework
{
    public static class ControllerContextExtensions
    {
        public static TextManager TextManager(this ControllerContext ctx)
        {
            return LocalizationHelper.TextManager;
        }
    }
}
