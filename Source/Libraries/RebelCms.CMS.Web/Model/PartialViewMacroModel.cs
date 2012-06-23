using RebelCms.Framework.Dynamics;

namespace RebelCms.Cms.Web.Model
{
    public class PartialViewMacroModel
    {

        public PartialViewMacroModel(Content page, BendyObject macroParams)
        {
            CurrentPage = page;
            MacroParameters = macroParams.AsDynamic();
        }

        public Content CurrentPage { get; private set; }

        public dynamic MacroParameters { get; private set; }

    }
}
