using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Web.Security;
using Rebel.Framework;
using Rebel.Framework.Dynamics;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Security
{
    public class RebelIdentity : FormsIdentity, IRebelIdentity
    {
        protected readonly string _userData;
        protected dynamic _deserializedData;

        private HiveId _id;
        private string[] _roles;

        public RebelIdentity(FormsAuthenticationTicket ticket) 
            : base(ticket)
        {
            _userData = ticket.UserData;
        }

        public HiveId Id
        {
            get
            {
                EnsureDeserialized();
                return _id != HiveId.Empty ? _id : (_id = HiveId.Parse((string)_deserializedData.Id));
            }
        }

        public string RealName
        {
            get
            {
                return _deserializedData.RealName;
            }
        }

        public int SessionTimeout
        {
            get
            {
                EnsureDeserialized();
                return _deserializedData.SessionTimeout;
            }
        }

        public string[] Roles
        {
            get
            {
                EnsureDeserialized();
                var roles = _deserializedData.Roles;
                if (roles == null || roles is BendyObject) return new string[0];
                return _roles ?? (_roles = ((object[])roles).WhereNotNull().Select(x => x.ToString()).ToArray());
            }
            internal set { _roles = value; }
        }

        protected void EnsureDeserialized()
        {
            if (_deserializedData != null)
                return;

            //create a bendey object from the user data
            if (string.IsNullOrEmpty(_userData))
            {
                _deserializedData = new BendyObject();
                return;
            }
            _deserializedData = new BendyObject((new JavaScriptSerializer()).Deserialize<IDictionary<string, object>>(_userData)).AsDynamic();
        }
    }
}
