using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Web.Mvc;
using RebelCms.Cms;
using RebelCms.Cms.Web.IO;
using RebelCms.Cms.Web.Model.BackOffice;

using RebelCms.Hive.ProviderGrouping;
using RebelCms.Hive.RepositoryTypes;
using Drawing = System.Drawing;
using Icon = RebelCms.Cms.Web.Model.Icon;

namespace RebelCms.Tests.Extensions
{
    public class MockedIconFileResolver : SpriteIconFileResolver
    {
        readonly IEnumerable<Icon> _icons = new List<Icon>
            {
                new Icon{Name= "Icon1"},
                new Icon{Name = "Icon2"}
            };

        public MockedIconFileResolver()
            : base(null, null)
        {
        }

        public override IEnumerable<Icon> Resolve()
        {
            return _icons;
        }

        public override string SpriteNamePrefix
        {
            get { return "blah-"; }
        }

        protected override Icon GetItem(FileInfo file)
        {
            return new Icon() {IconType = IconType.Image, IconSize = new Size(10, 10), Name = "Blah"};
        }
    }
}