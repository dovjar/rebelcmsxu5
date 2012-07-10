using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Rebel.Cms.Web.Surface;
using Rebel.Cms.Web.UI.Models;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Security;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.Entities;
using Rebel.Framework.Security.Model.Entities;
using Rebel.Hive;
using Rebel.Hive.RepositoryTypes;
using Rebel.Framework.Persistence;

namespace Rebel.Cms.Web.UI.Controllers
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
                return CurrentRebelPage();
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

                return RedirectToCurrentRebelPage();
            }

            ModelState.AddModelError("LoginForm.", "Username and/or Password were invalid. Please try again.");

            return CurrentRebelPage();
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
                return CurrentRebelPage();
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
                    return CurrentRebelPage();
                }

                if(!model.SuccessRedirectId.IsNullValueOrEmpty())
                {
                    return Redirect(_context.RoutingEngine.GetUrl(model.SuccessRedirectId));
                }

                return RedirectToCurrentRebelPage();
            }
        }
    }
}
