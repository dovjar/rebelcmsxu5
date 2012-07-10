using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rebel.Cms.Web.Model.BackOffice.UIElements
{
    public interface IHasUIElements
    {
        IList<UIElement> UIElements { get; }
    }
}
