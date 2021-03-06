﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rebel.Cms.Web.Model;
using Rebel.Cms.Web.Routing;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model.Attribution;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Rebel.Framework.Persistence.Model.Constants.Entities;
using Rebel.Hive;
using Rebel.Hive.RepositoryTypes;
using Rebel.Tests.Cms.Stubs;
using Rebel.Cms.Web.Mvc.Controllers;
using System.Web.Routing;
using Rebel.Cms.Web.Mvc.Controllers.BackOffice;
using System.Xml.Linq;
using System.Reflection;
using System.IO;
using System.Web.Mvc;
using MvcContrib.TestHelper;
using System.Web;
using Rhino.Mocks;
using Rebel.Tests.Extensions;
using Rebel.Tests.Extensions.Stubs;


namespace Rebel.Tests.Cms.Mvc.Routing
{

    //NOTE: Dont' use the MVCContrib Route() methods or the string.ShouldMapTo<> methods... this is because the FakeHttpContext that
    // it uses has issues. I've logged a bug regarding this on CP.


    [TestFixture]
    public class FrontEndRouting : AbstractRoutingTest
    {
        /// <summary>
        /// Check ignore routes
        /// </summary>
        [TestCase("~/favicon.ico")]
        [TestCase("~/SomeFolder/AnotherFolder/favicon.ico?sdf=1dfsd")]
        [TestCase("~/DependencyHandler.axd?s=asdfasdfasdfasdf")]
        [TestCase("~/SomeFolder/DependencyHandler.axd?s=asdfasdfasdfasdf&cdv=123")]
        [TestCase("~/SomeFolder/MyPage.aspx")]
        [TestCase("~/MyWebformsPage.aspx?y=o&u=1&e=6")]
        [TestCase("~/I/Am/In/a/Folder/AndImAhandler.ashx")]
        [TestCase("~/I/Am/In/a/Folder/AndImAhandler.ashx?with=a&query=string")]        
        [Test]
        [Category(TestOwner.CmsFrontend)]
        public void FrontEndRouting_Ignore_Routes(string url)
        {
            url.ShouldIgnoreRoute();
        }

        /// <summary>
        /// Test paths that aren't front-end routable that map to user defined controllers/actions
        /// </summary>
        [Test]
        [Category(TestOwner.CmsFrontend)]
        public void FrontEndRouting_User_Defined_Pages()
        {         
            RouteTable.Routes.GetDataForRoute("~/CustomUser/").ShouldMapTo<CustomUserController>(x => x.Index());
            RouteTable.Routes.GetDataForRoute("~/CustomUser/Test/5").ShouldMapTo<CustomUserController>(x => x.Test(5));
        }

        /// <summary>
        /// Test Rebel front-end routable paths
        /// </summary>
        [TestCase("~/", "/")]
        [TestCase("~/runway-homepage", "/runway-homepage")]
        [TestCase("~/runway-homepage/installing-runway-modules", "/runway-homepage/installing-runway-modules")]
        [TestCase("~/runway-homepage/faq/functionality", "/runway-homepage/faq/functionality")]        
        [Test]
        [Category(TestOwner.CmsFrontend)]
        public void FrontEndRouting_Rebel_Pages(string routeUrl, string matchUrl)
        {            
            //set the fake render model factory to match "/" for the specified content item
            RenderModelFactory.AddMatchingUrlForContent(matchUrl, new Content(new HiveId(Guid.NewGuid()), new List<TypedAttribute>()));

            RouteTable.Routes.GetDataForRoute(routeUrl).ShouldMapTo<RebelController>(x => x.Index((IRebelRenderModel)null));
        }

        [TestCase("~/MyCustomMvcPage")]
        [TestCase("~/Home/Index/1234")]
        [TestCase("~/SomeOtherPage/Home")]
        [TestCase("~/SomeOtherPage/Home/FFF?e=123")]
        [Test]
        [Category(TestOwner.CmsFrontend)]
        public void FrontEndRouting_Non_Rebel_Pages(string url)
        {
            var route = RouteTable.Routes.GetDataForRoute(url);
            
            Assert.AreNotEqual("Rebel", route.Values["controller"].ToString());
        }

        /// <summary>
        /// Test for an Rebel route that has a user defined controller
        /// </summary>
        [Test]
        [Category(TestOwner.CmsFrontend)]
        public void FrontEndRouting_Rebel_Route_User_Defined_Controller()
        {
            var http = new FakeHttpContextFactory("~/some-page");
            var content = new Content(new HiveId(Guid.NewGuid()), new List<TypedAttribute>());
            content.CurrentTemplate = new Template(new HiveId(new Uri("storage://templates"), "", new HiveIdValue("text-page.cshtml")));
            content.ContentType = new EntitySchema("customRebel");

            RenderModelFactory.AddMatchingUrlForContent("/some-page", content);

            var data = RouteTable.Routes.GetDataForRoute(http.HttpContext);
            Assert.AreEqual(typeof(RenderRouteHandler), data.RouteHandler.GetType());
            var handler = (RenderRouteHandler) data.RouteHandler;
            handler.GetHandlerForRoute(http.RequestContext, RenderModelFactory.Create(http.HttpContext, http.HttpContext.Request.RawUrl));
            Assert.AreEqual("CustomRebel", data.Values["controller"].ToString());
            Assert.AreEqual("Index", data.Values["action"].ToString());
        }

        /// <summary>
        /// Test for an Rebel route that has a user defined controller and action
        /// </summary>
        [Test]
        [Category(TestOwner.CmsFrontend)]
        public void FrontEndRouting_Rebel_Route_User_Defined_Controller_Action()
        {
            var http = new FakeHttpContextFactory("~/home-page");
            var content = new Content(new HiveId(Guid.NewGuid()), new List<TypedAttribute>());
            content.CurrentTemplate = new Template(new HiveId(new Uri("storage://templates"), "", new HiveIdValue("home-page.cshtml")));
            content.ContentType = new EntitySchema("customRebel");

            RenderModelFactory.AddMatchingUrlForContent("/home-page", content);

            var data = RouteTable.Routes.GetDataForRoute(http.HttpContext);
            Assert.AreEqual(typeof(RenderRouteHandler), data.RouteHandler.GetType());
            var handler = (RenderRouteHandler)data.RouteHandler;
            handler.GetHandlerForRoute(http.RequestContext, RenderModelFactory.Create(http.HttpContext, http.HttpContext.Request.RawUrl));
            Assert.AreEqual("CustomRebel", data.Values["controller"].ToString());
            Assert.AreEqual("HomePage", data.Values["action"].ToString());
        }

        

        #region Internal classes

        /// <summary>
        /// Used to test a user route (non-rebel)
        /// </summary>
        private class CustomUserController : Controller
        {

            public ActionResult Index()
            {
                return View();
            }

            public ActionResult Test(int id)
            {
                return View();
            }

        }

        /// <summary>
        /// Used to test a user route rebel route
        /// </summary>
        private class CustomRebelController : RebelController
        {
            
            public ActionResult HomePage(IRebelRenderModel model)
            {
                return View();
            }

        } 

        #endregion
    }



}
