using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Rebel.Cms.Web.Configuration;

namespace Rebel.Cms.Web.IO
{
    public class ApplicationIconFileResolver : SpriteIconFileResolver
    {
        public const string SpriteNamePrefixValue = "app-";

        public ApplicationIconFileResolver(HttpServerUtilityBase server, RebelSettings settings, UrlHelper url)
            : base(new DirectoryInfo(server.MapPath(settings.RebelFolders.ApplicationIconFolder)), url)
        {
        }

        public override string SpriteNamePrefix
        {
            get { return SpriteNamePrefixValue; }
        }

    }
}