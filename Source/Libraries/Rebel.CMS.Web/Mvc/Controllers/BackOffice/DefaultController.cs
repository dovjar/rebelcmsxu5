using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using System.Web.Security;
using ClientDependency.Core;
using ClientDependency.Core.Mvc;
using Rebel.Cms.Web.Configuration.ApplicationSettings;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Editors;
using System.Linq;
using Rebel.Cms.Web.IO;
using Rebel.Cms.Web.Model.BackOffice;
using Rebel.Cms.Web.Model.BackOffice.Editors;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Cms.Web.Security;
using Rebel.Cms.Web.Trees;
using Rebel.Framework.Localization;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Security;
using BackOfficeDefaultModel = Rebel.Cms.Web.Model.BackOffice.BackOfficeDefaultModel;

namespace Rebel.Cms.Web.Mvc.Controllers.BackOffice
{
    

    /// <summary>
    /// The controller used to render the back office application
    /// </summary>
    /// <remarks>
    /// We need to ensure that each action has the explicit RebelAuthorize attribute applied (except for Login) because this does
    /// not inherit from SecuredBackOfficeController
    /// </remarks>
    public class DefaultController : BackOfficeController
    {
        private readonly IEnumerable<Lazy<MenuItem, MenuItemMetadata>> _menuItems;

        /// <summary>
        /// 
        /// </summary>
        public DefaultController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {            
            _menuItems = requestContext.RegisteredComponents.MenuItems;         
        }

        /// <summary>
        /// Renders the login view
        /// </summary>
        /// <returns></returns>
        //[InstalledFilter]
        //public ActionResult Login()
        //{
        //    return Login(LoginDisplayType.StandardPage);
        //}

        /// <summary>
        /// Renders the login view
        /// </summary>
        /// <returns></returns>
        [InstalledFilter]
        public ActionResult Login(LoginDisplayType displayType = LoginDisplayType.StandardPage)
        {
            return View(new LoginModel() { DisplayType = displayType});
        }

        /// <summary>
        /// Renders the generic Search list template
        /// </summary>
        /// <returns></returns>
        [InstalledFilter(Order = 0)]
        [RebelAuthorize(Order = 1)]
        public ActionResult Search(string searchTerm, Guid treeId)
        {

            return View();
        }

        [HttpPost]
        [ActionName("Login")]
        [InstalledFilter]
        public ActionResult LoginForm(LoginModel model)
        {
            model.Username = model.Username.Trim();
            model.Password = model.Password.Trim();

            var membershipService = BackOfficeRequestContext.Application.Security.Users;
            
            if (membershipService.Validate(model.Username, model.Password))
            {
                var user = membershipService.GetByUsername(model.Username, false);

                var userData = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<UserData>(user);

                HttpContext.CreateRebelAuthTicket(userData);

                //check if the ReturnUrl is specified in the route
                    var redirectUrl = HttpContext.Request["ReturnUrl"];
                    if (!string.IsNullOrEmpty(redirectUrl) &&
                        !(redirectUrl.StartsWith("http") && !redirectUrl.StartsWith(HttpContext.Request.Url.Scheme + "://" + HttpContext.Request.Url.Authority)))
                {
                    return Redirect(HttpContext.Request["ReturnUrl"]);
                }

                    //check if it is an overlay login (auth timeout)
                    if (model.DisplayType == LoginDisplayType.DisplayingOverlay)
                    {
                        return View("LoginOverlaySuccess");
                    }

                return RedirectToAction("App");
            }

            ModelState.AddModelError("Login.Invalid", "Login.Invalid".Localize(this));

            return View(model);
        }

        [RebelAuthorize]
        public ActionResult Logout()
        {
            HttpContext.RebelLogout();

            return RedirectToAction("Login");
        }

        /// <summary>
        /// Renders the default back-office View
        /// </summary>
        /// <param name="appAlias">The application to render</param>
        /// <returns></returns>
        [InstalledFilter(Order = 0)]
        [RebelAuthorize(Permissions = new[] { FixedPermissionIds.BackOfficeAccess }, Order = 1)]
        [HandleAuthorizationErrors(false)]
        public virtual ActionResult App(string appAlias)
        {
            
            //create the model to return
            var model = new BackOfficeDefaultModel
                            {
                                //Applications = apps,
                                TreeModel = new TreeRenderModel(Url.GetTreeUrl(appAlias)) { ManuallyInitialize = true },
                                //DefaultDashboardEditor = editor,
                                CurrentApplicationAlias = appAlias,
                                MenuItems = _menuItems.Select(x => x.Value)
                            };

            return View("Index", model);
        }

        public PartialViewResult ApplicationTray()
        {
            var currentUser = (RebelBackOfficeIdentity)HttpContext.User.Identity;

            //get the apps that the user is allowed to view
            var apps = BackOfficeRequestContext.Application.FrameworkContext
                .TypeMappers.Map<IEnumerable<IApplication>, IEnumerable<ApplicationTrayModel>>(
                    BackOfficeRequestContext.Application.Settings.Applications)
                .Where(x => currentUser.AllowedApplications.Any(y => y == x.Alias));

            //set the tree Render Model and dashboard url for each app
            foreach (var a in apps)
            {
                a.TreeModel = new TreeRenderModel(Url.GetTreeUrl(a.Alias));
                a.DashboardUrl = Url.GetDashboardUrl(a.Alias);
            }
            return PartialView("TrayPartial", apps);
        }

        /// <summary>
        /// Insufficient the permissions.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public virtual ActionResult InsufficientPermissions(string url)
        {
            ViewBag.Url = url;

            return View();
        }
    }
}
