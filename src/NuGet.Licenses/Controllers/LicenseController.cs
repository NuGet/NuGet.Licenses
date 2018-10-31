// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web.Mvc;
using NuGet.Packaging.Licenses;

namespace NuGet.Licenses.Controllers
{
    public class LicenseController : Controller
    {
        public ActionResult Index()
        {
            return Redirect("https://github.com/NuGet/Home/wiki/Packaging-License-within-the-nupkg");
        }

        public ActionResult DisplayLicense(string licenseExpression)
        {
            NuGetLicenseExpression licenseExpressionRootNode;

            try
            {
                licenseExpressionRootNode = NuGetLicenseExpression.Parse(licenseExpression);
            }
            catch (NuGetLicenseExpressionParsingException e)
            {
                throw new InvalidOperationException();
            }

            if (licenseExpressionRootNode.Type == LicenseExpressionType.License)
            {
                var license = licenseExpressionRootNode as NuGetLicense;

                // TODO: if license == null then something went really wrong

                if (!license.IsStandardLicense)
                {
                    // TODO: 404?
                }

                // root of the message can only be license if it's a simple license expression.
                return DisplayLicense(license);
            }
            else if (licenseExpressionRootNode.Type == LicenseExpressionType.Operator)
            {
                var complexLicenseExpressionRoot = licenseExpressionRootNode as LicenseOperator;

                // TODO: if licenseExpressionRoot == null something went really wrong.

                return Content(licenseExpression);
            }

            return Content("WAT");
        }

        private ActionResult DisplayLicense(NuGetLicense license)
        {
            return Content("license: " + license.Identifier);
        }

        private ActionResult DisplayComplexLicenseExpression(LicenseOperator licenseExpressionRoot)
        {

        }
    }
}