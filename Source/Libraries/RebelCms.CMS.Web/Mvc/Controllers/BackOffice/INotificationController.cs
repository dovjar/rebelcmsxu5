using System;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Model.BackOffice.Editors;

namespace RebelCms.Cms.Web.Mvc.Controllers.BackOffice
{
    /// <summary>
    /// Required to use client notification engine
    /// </summary>
    public interface INotificationController
    {
        ClientNotifications Notifications { get; }
     
        /// <summary>
        /// The Id of the current request
        /// </summary>
        Guid RequestId { get; }
    }
}