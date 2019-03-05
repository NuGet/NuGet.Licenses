// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Mvc;
using System.Web.Routing;

namespace NuGet.Licenses
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute(
                name: "Root",
                url: "",
                defaults: new { controller = "License", action = "Index"});

            routes.MapRoute(
                name: "Errors",
                url: "Error",
                defaults: new { controller = "Error", action = "Index" });

            routes.MapRoute(
                name: "LicenseExpression",
                url: "{licenseExpression}",
                defaults: new { controller = "License", action = "DisplayLicense" });
        }
    }
}
