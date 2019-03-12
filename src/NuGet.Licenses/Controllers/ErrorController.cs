// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web.Mvc;

namespace NuGet.Licenses.Controllers
{
    public class ErrorController : Controller
    {
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Head)]
        public ActionResult Index()
        {
            return View("Error");
        }
    }
}
