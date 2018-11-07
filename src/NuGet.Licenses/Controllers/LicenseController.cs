// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web.Mvc;
using NuGet.Licenses.Models;
using NuGet.Licenses.Services;
using NuGet.Packaging.Licenses;

namespace NuGet.Licenses.Controllers
{
    public class LicenseController : Controller
    {
        private readonly ILicenseFileService _licenseFileService;

        public LicenseController(
            ILicenseFileService licenseFileService
            )
        {
            _licenseFileService = licenseFileService;    
        }

        public ActionResult Index()
        {
            return Redirect("https://github.com/NuGet/Home/wiki/Packaging-License-within-the-nupkg");
        }

        public ActionResult DisplayLicense(string licenseExpression)
        {
            NuGetLicenseExpression licenseExpressionRootNode;

            if (licenseExpression == null || licenseExpression.Length > 500)
            {
                return InvalidRequest();
            }

            try
            {
                // TODO: license expression might be an exception
                licenseExpressionRootNode = NuGetLicenseExpression.Parse(licenseExpression);
            }
            catch (NuGetLicenseExpressionParsingException e)
            { 
                throw new InvalidOperationException($"Failed to parse license expression: {licenseExpression}", e);
            }

            if (licenseExpressionRootNode.Type == LicenseExpressionType.License)
            {
                var license = licenseExpressionRootNode as NuGetLicense;

                // TODO: if license == null then something went really wrong

                if (!license.IsStandardLicense)
                {
                    return UnknownLicense(license);
                }

                // root of the message can only be license if it's a simple license expression.
                return DisplayLicense(license);
            }
            else if (licenseExpressionRootNode.Type == LicenseExpressionType.Operator)
            {
                var complexLicenseExpressionRoot = licenseExpressionRootNode as LicenseOperator;

                // TODO: if licenseExpressionRoot == null something went really wrong.

                return DisplayComplexLicenseExpression(complexLicenseExpressionRoot, licenseExpression);
                //return Content(licenseExpression);
            }

            throw new InvalidOperationException($"Unexpected license expression root node type: {licenseExpressionRootNode.Type}");
        }

        private ActionResult InvalidRequest()
        {
            Response.StatusCode = 400;
            return View("InvalidRequest");
        }

        private ActionResult UnknownLicense(NuGetLicense license)
        {
            Response.StatusCode = 404;
            return View("UnknownLicense", new UnknownLicenseModel(license.Identifier));
        }

        private ActionResult DisplayLicense(NuGetLicense license)
        {
            if (license == null)
            {
                throw new ArgumentNullException(nameof(license));
            }

            string licenseFilePath = _licenseFileService.GetLicenseFilePath(license.Identifier);
            if (!_licenseFileService.IsLicenseFilePathAllowed(licenseFilePath))
            {
                return InvalidRequest();
            }

            if (!_licenseFileService.IsLicenseFileExisted(licenseFilePath))
            {
                return UnknownLicense(license);
            }

            string licenseContent = _licenseFileService.GetLicenseFileContent(licenseFilePath);
            return View(new SingleLicenseInformationModel(license.Identifier, licenseContent));
        }

        private ActionResult DisplayComplexLicenseExpression(LicenseOperator licenseExpressionRoot, string licenseExpression)
        {
            // TODO: DI this
            var splitter = new LicenseExpressionSplitter();
            var runs = splitter.GetLicenseExpressionRuns(licenseExpressionRoot);
            var fullRuns = splitter.SplitFullExpression(licenseExpression, runs);

            return View("ComplexLicenseExpression", new ComplexLicenseExpressionViewModel(fullRuns));
        }
    }
}