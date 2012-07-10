using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Web.Mvc;
using Rebel.Cms;
using Rebel.Cms.Web.IO;
using Rebel.Cms.Web.Model.BackOffice;

using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.RepositoryTypes;
using Drawing = System.Drawing;
using Icon = Rebel.Cms.Web.Model.Icon;

namespace Rebel.Tests.Extensions
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