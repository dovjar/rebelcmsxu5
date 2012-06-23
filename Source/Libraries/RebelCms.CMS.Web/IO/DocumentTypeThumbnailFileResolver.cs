using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using RebelCms.Cms.Web.Configuration;

using RebelCms.Hive;
using RebelCms.Hive.Configuration;
using RebelCms.Hive.ProviderGrouping;
using RebelCms.Hive.RepositoryTypes;

namespace RebelCms.Cms.Web.IO
{
    public class DocumentTypeThumbnailFileResolver : SpriteIconFileResolver
    {
        public const string SpriteNamePrefixValue = "thumb-";

        public DocumentTypeThumbnailFileResolver(HttpServerUtilityBase server, RebelCmsSettings settings, UrlHelper url)
            : base(new DirectoryInfo(server.MapPath(settings.RebelCmsFolders.DocTypeThumbnailFolder)), url)
        {
        }

        public override string SpriteNamePrefix
        {
            get { return SpriteNamePrefixValue; }
        }
    }
}