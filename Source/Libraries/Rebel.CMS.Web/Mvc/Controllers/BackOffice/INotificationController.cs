using System;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model.BackOffice.Editors;

namespace Rebel.Cms.Web.Mvc.Controllers.BackOffice
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