// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Autofac;
using Autofac.Integration.Mvc;
using Microsoft.ApplicationInsights.Extensibility;
using System.Net;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace NuGet.Licenses
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            SetupInstrumentation();
            SetupSsl();
            SetupDi();

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        private static void SetupInstrumentation()
        {
            TelemetryConfiguration.Active.InstrumentationKey = WebConfigurationManager.AppSettings["Licenses.InstrumentationKey"];
        }

        private static void SetupSsl()
        {
            // Ensure that SSLv3 is disabled and that Tls v1.2 is enabled.
            ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Ssl3;
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        }

        private static void SetupDi()
        {
            var builder = new ContainerBuilder();
            builder.RegisterControllers(typeof(MvcApplication).Assembly);
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}
