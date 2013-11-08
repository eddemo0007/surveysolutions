﻿using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Elmah;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Logging;
using WB.UI.Designer.App_Start;
using WB.UI.Designer.Controllers;
using WB.UI.Shared.Web.Elmah;

namespace WB.UI.Designer
{
    using NConfig;

    //using WB.Core.SharedKernel.Utils.NLog;

    public class MvcApplication : HttpApplication
    {
        /// <summary>
        /// Initialization per AppDomain.
        /// </summary>
        static MvcApplication()
        {
            SetupNConfig();

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            RouteConfig.RegisterRoutes(RouteTable.Routes);

            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ValidatorsConfig.Register();

            //BundleTable.EnableOptimizations = true;
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var httpContext = ((MvcApplication)sender).Context;

            var ex = Server.GetLastError();

            var logger = ServiceLocator.Current.GetInstance<ILogger>();
            logger.Error("Unexpected error occurred", ex);

            var controller = new ErrorController();
            var routeData = new RouteData();
            var action = "Index";

            if (ex is HttpAntiForgeryException)
            {
                httpContext.Response.Redirect(httpContext.Request.Url.ToString(), true);
                return;
            }

            if (ex is HttpException)
            {
                var httpEx = ex as HttpException;

                switch (httpEx.GetHttpCode())
                {
                    case 404:
                        action = "NotFound";
                        break;
                    case 403:
                        action = "Forbidden";
                        break;
                    case 401:
                        action = "AccessDenied";
                        break;
                }
            }

            var currentController = string.Empty;
            var currentAction = string.Empty;
            var currRouteData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));
            if (currRouteData != null)
            {
                currentController = (string)currRouteData.Values["controller"] ?? string.Empty;
                currentAction = (string)currRouteData.Values["action"] ?? string.Empty;
            }

            httpContext.ClearError();
            httpContext.Response.Clear();
            httpContext.Response.StatusCode = ex is HttpException ? ((HttpException)ex).GetHttpCode() : 500;
            httpContext.Response.TrySkipIisCustomErrors = true;

            routeData.Values["controller"] = "Error";
            routeData.Values["action"] = action;

            controller.ViewData.Model = new HandleErrorInfo(ex, currentController, currentAction);
            ((IController)controller).Execute(new RequestContext(new HttpContextWrapper(httpContext), routeData));
        }

        private static void SetupNConfig()
        {
            NConfigurator.UsingFiles(@"~\Configuration\Designer.Web.config").SetAsSystemDefault();
        }

        #warning TLK: delete this when NCQRS initialization moved to Global.asax
        public static void Initialize() { }

        void ErrorLog_Filtering(object sender, ExceptionFilterEventArgs e)
        {
            var ctx = e.Context as HttpContext;
            if (ctx == null)
            {
                return;
            }
            ElmahDataFilter.Apply(e, ctx);
        }
    }
}