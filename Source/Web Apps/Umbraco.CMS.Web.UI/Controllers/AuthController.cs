using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Umbraco.Cms.Web.Surface;
using Umbraco.Cms.Web.UI.Models;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Security;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Security.Model.Entities;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;
using Umbraco.Framework.Persistence;

namespace Umbraco.Cms.Web.UI.Controllers
{
    public class AuthSurfaceController : SurfaceController
    {
        private IRoutableRequestContext _context;

        public AuthSurfaceController(IRoutableRequestContext context)
        {
            _context = context;
        }

        [ChildActionOnly]
        public PartialViewResult Login(HiveId defaultRedirectId)
        {
            var viewModel = new LoginModel { DefaultRedirectId = defaultRedirectId, ReturnUrl = HttpContext.Request["ReturnUrl"] };

            return PartialView("LoginPartial", viewModel);
        }

        [HttpPost]
        public ActionResult LoginForm([Bind(Prefix = "LoginForm")]LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            if (_context.Application.Security.Members.Validate(model.Username, model.Password, true))
            {
                FormsAuthentication.SetAuthCookie(model.Username, false);

                var redirectUrl = model.ReturnUrl;
                if (!string.IsNullOrEmpty(redirectUrl) && !(redirectUrl.StartsWith("http") && !redirectUrl.StartsWith(HttpContext.Request.Url.Scheme + "://" + HttpContext.Request.Url.Authority)))
                {
                    return Redirect(redirectUrl);
                }

                if(!model.DefaultRedirectId.IsNullValueOrEmpty())
                {
                    return Redirect(_context.RoutingEngine.GetUrl(model.DefaultRedirectId));
                }

                return RedirectToCurrentUmbracoPage();
            }

            ModelState.AddModelError("LoginForm.", "Username and/or Password were invalid. Please try again.");

            return CurrentUmbracoPage();
        }

        [HttpGet]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            return Redirect(Request.UrlReferrer.ToString());
        }


        [ChildActionOnly]
        public PartialViewResult Register(string memberTypeAlias, HiveId memberGroupId, HiveId successRedirectId)
        {
            var viewModel = new RegisterModel
            {
                MemberTypeAlias = memberTypeAlias, 
                MemberGroupId = memberGroupId,
                SuccessRedirectId = successRedirectId,
                RequiresQuestionAndAnswer = _context.Application.Security.Members.MembershipProvider.RequiresQuestionAndAnswer
            };

            return PartialView("RegisterPartial", viewModel);
        }

        [HttpPost]
        public ActionResult RegisterForm([Bind(Prefix = "RegisterForm")]RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            //  Create a member
            using (var uow = _context.Application.Hive.OpenWriter<ISecurityStore>())
            {
                var member = new Member();
                member.SetupFromSchema(model.MemberTypeAlias, uow);
                member.Name = model.Name;
                member.Username = model.Username;
                member.Email = model.Email;
                member.Password = model.Password;
                member.IsApproved = true;
                member.Groups = new[] { model.MemberGroupId };

                MembershipCreateStatus status;
                _context.Application.Security.Members.Create(member, out status);

                if (status != MembershipCreateStatus.Success)
                {
                    ModelState.AddModelError("RegisterForm.", "Error creating member: " + status.ToString());
                    return CurrentUmbracoPage();
                }

                if(!model.SuccessRedirectId.IsNullValueOrEmpty())
                {
                    return Redirect(_context.RoutingEngine.GetUrl(model.SuccessRedirectId));
                }

                return RedirectToCurrentUmbracoPage();
            }
        }
    }
}
