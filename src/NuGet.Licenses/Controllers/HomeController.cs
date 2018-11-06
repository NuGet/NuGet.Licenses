// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web.Mvc;
using Microsoft.Extensions.Logging;

namespace NuGet.Licenses.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ActionResult Index()
        {
            return Redirect("https://github.com/NuGet/Home/wiki/Packaging-License-within-the-nupkg");
        }
    }
}