﻿using System.Web.Mvc;
using System.Web.Routing;
using System.Web.WebPages.Scope;
using System.Reflection;

namespace Tychaia.Website
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{resource}.axd");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_BeginRequest()
        {
            // http://www.codinghub.net/2012/12/storage-scopes-cannot-be-created-when.html
            var ob = typeof(
                AspNetRequestScopeStorageProvider).Assembly.GetType(
                "System.Web.WebPages.WebPageHttpModule").GetProperty
                ("AppStartExecuteCompleted", 
                 BindingFlags.NonPublic | BindingFlags.Static);
            ob.SetValue(null, true, null);
        }
    }
}
