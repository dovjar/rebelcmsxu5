using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using System.Web.Security;
using Umbraco.Framework;
using Umbraco.Framework.Dynamics;
using System;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Security
{

    /// <summary>
    /// A custom user identity for the Umbraco backoffice
    /// </summary>
    /// <remarks>
    /// All values are lazy loaded for performance reasons as the constructor is called for every single request
    /// </remarks>
    public class UmbracoBackOfficeIdentity : UmbracoIdentity, IUmbracoBackOfficeIdentity
    {
        public UmbracoBackOfficeIdentity(FormsAuthenticationTicket ticket)
            : base(ticket)
        { }

        private HiveId _startContentNode;
        private HiveId _startMediaNode;
        private string[] _allowedApplications;

        public HiveId StartContentNode
        {
            get
            {
                EnsureDeserialized();
                return _startContentNode != HiveId.Empty ? _startContentNode : (_startContentNode = HiveId.Parse((string)_deserializedData.StartContentNode));
            }
        }

        public HiveId StartMediaNode
        {
            get
            {
                EnsureDeserialized();
                return _startMediaNode != HiveId.Empty ? _startMediaNode : (_startMediaNode = HiveId.Parse((string)_deserializedData.StartMediaNode));
            }
        }

        public string[] AllowedApplications
        {
            get
            {
                EnsureDeserialized();
                return _allowedApplications ?? (_allowedApplications = ((object[])_deserializedData.AllowedApplications).Select(x => x.ToString()).ToArray());
            }
        }
    }
}