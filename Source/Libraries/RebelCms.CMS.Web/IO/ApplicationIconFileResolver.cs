using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using RebelCms.Cms.Web.Configuration;

namespace RebelCms.Cms.Web.IO
{
    public class ApplicationIconFileResolver : SpriteIconFileResolver
    {
        public const string SpriteNamePrefixValue = "app-";

        public ApplicationIconFileResolver(HttpServerUtilityBase server, RebelCmsSettings settings, UrlHelper url)
            : base(new DirectoryInfo(server.MapPath(settings.RebelCmsFolders.ApplicationIconFolder)), url)
        {
        }

        public override string SpriteNamePrefix
        {
            get { return SpriteNamePrefixValue; }
        }

    }
}