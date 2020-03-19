// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using NuGet.Licenses.Services;
using Serilog;

namespace NuGet.Licenses
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            SetupInstrumentation();
            SetupSsl();
            SetupLogging();
            SetupDependencyInjection();

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        private static string InstrumentationKey => WebConfigurationManager.AppSettings["Licenses.InstrumentationKey"];

        private void SetupLogging()
        {
            var loggerConfiguration = new LoggerConfiguration();

            if (!string.IsNullOrWhiteSpace(InstrumentationKey))
            { 
                loggerConfiguration = loggerConfiguration
                    .WriteTo
                    .ApplicationInsights(TelemetryConfiguration.Active, TelemetryConverter.Traces);
            }

            Log.Logger = loggerConfiguration.CreateLogger();
        }

        private static void SetupInstrumentation()
        {
            TelemetryConfiguration.Active.InstrumentationKey = InstrumentationKey;
        }

        private static void SetupSsl()
        {
            // Ensure that SSLv3 is disabled and that Tls v1.2 is enabled.
            ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Ssl3;
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        }

        private static void SetupDependencyInjection()
        {
            var builder = new ContainerBuilder();
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            var loggerFactory = new LoggerFactory();
            loggerFactory.AddSerilog();
            builder
                .RegisterInstance(loggerFactory)
                .As<ILoggerFactory>();

            builder
                .RegisterGeneric(typeof(Logger<>))
                .As(typeof(ILogger<>))
                .SingleInstance();

            builder
                .RegisterType<LicenseExpressionSegmentator>()
                .As<ILicenseExpressionSegmentator>()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<LicenseFileService>()
                .As<ILicenseFileService>()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<LicensesFolderPathService>()
                .As<ILicensesFolderPathService>()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<FileService>()
                .As<IFileService>()
                .InstancePerLifetimeScope();

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}
