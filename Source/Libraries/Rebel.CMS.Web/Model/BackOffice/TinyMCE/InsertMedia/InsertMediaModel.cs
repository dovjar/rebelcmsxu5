using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Framework;
using Rebel.Framework.Dynamics;

namespace Rebel.Cms.Web.Model.BackOffice.TinyMCE.InsertMedia
{
    public class InsertMediaModel
    {
        public InsertMediaModel(HiveId mediaId, string mediaFilePath, BendyObject mediaParameters)
        {
            MediaId = mediaId;
            MediaParameters = mediaParameters.AsDynamic();
            MediaFilePath = mediaFilePath;
        }

        public HiveId MediaId { get; set; }

        public string FilePropertyAlias { get; set; }

        public string MediaFilePath { get; set; }

        public dynamic MediaParameters { get; private set; }
    }
}
