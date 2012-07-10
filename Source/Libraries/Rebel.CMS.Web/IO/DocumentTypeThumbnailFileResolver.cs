using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Rebel.Cms.Web.Configuration;

using Rebel.Hive;
using Rebel.Hive.Configuration;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Cms.Web.IO
{
    public class DocumentTypeThumbnailFileResolver : SpriteIconFileResolver
    {
        public const string SpriteNamePrefixValue = "thumb-";

        public DocumentTypeThumbnailFileResolver(HttpServerUtilityBase server, RebelSettings settings, UrlHelper url)
            : base(new DirectoryInfo(server.MapPath(settings.RebelFolders.DocTypeThumbnailFolder)), url)
        {
        }

        public override string SpriteNamePrefix
        {
            get { return SpriteNamePrefixValue; }
        }
    }
}