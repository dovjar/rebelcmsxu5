using System;
using RebelCms.Cms.Web.Security.Permissions;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Model.BackOffice.Editors
{
    public class PermissionStatusModel
    {
        public PermissionStatusModel()
        {
            Status = PermissionStatus.Inherit;
        }

        public Guid PermissionId { get; set; }
        public string PermissionName { get; set; }
        public PermissionStatus? Status { get; set; }
        public PermissionStatus? InheritedStatus { get; set; }
    }
}