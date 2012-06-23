using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RebelCms.Cms.Web.Model;

namespace RebelCms.Cms.Web
{
    public static class ContentExtensions
    {
        /// <summary>
        /// Tries to swap the template of a Content item to the alt template with the supplied alias.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="altTemplateAlias">The alt template alias.</param>
        /// <returns></returns>
        public static bool TrySwapTemplate(this Content content, string altTemplateAlias)
        {
            if (!string.IsNullOrWhiteSpace(altTemplateAlias))
            {
                var altTemplate = content.AlternativeTemplates.SingleOrDefault(x => x.Alias != null && x.Alias.Equals(altTemplateAlias, StringComparison.InvariantCultureIgnoreCase));
                if (altTemplate != null)
                {
                    content.CurrentTemplate = altTemplate;
                    return true;
                }
            }

            return false;
        }
    }
}
