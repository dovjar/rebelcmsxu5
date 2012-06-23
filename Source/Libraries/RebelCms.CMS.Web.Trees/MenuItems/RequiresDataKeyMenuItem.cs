using System;
using System.Collections.Generic;
using System.Linq;
using RebelCms.Cms.Web.Model.BackOffice.Trees;

namespace RebelCms.Cms.Web.Trees.MenuItems
{
    /// <summary>
    /// A menu item that validates that a required key exists in the additional data
    /// </summary>
    public abstract class RequiresDataKeyMenuItem : MenuItem
    {
        public abstract string[] RequiredKeys { get; }

        public override void ValidateRequiredData(IDictionary<string, object> additionalData)
        {
            if (RequiredKeys.Any(r => !additionalData.ContainsKey(r)))
            {
                throw new ArgumentNullException("A " + RequiredKeys + " must exist in the tree node's AdditionalData when using the " + GetType() + " menu object");
            }
        }
    }
}