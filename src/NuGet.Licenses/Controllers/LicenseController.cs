// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web.Mvc;
using Microsoft.Extensions.Logging;
using NuGet.Licenses.Models;
using NuGet.Licenses.Services;
using NuGet.Packaging.Licenses;

namespace NuGet.Licenses.Controllers
{
    public class LicenseController : Controller
    {
        private readonly ILicenseExpressionSplitter _licenseExpressionSplitter;
        private readonly ILogger<LicenseController> _logger;
        private readonly ILicenseFileService _licenseFileService;

        public LicenseController(
            ILicenseExpressionSplitter licenseExpressionSplitter,
            ILogger<LicenseController> logger,
            ILicenseFileService licenseFileService)
        {
            _licenseExpressionSplitter = licenseExpressionSplitter ?? throw new ArgumentNullException(nameof(licenseExpressionSplitter));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _licenseFileService = licenseFileService ?? throw new ArgumentNullException(nameof(licenseFileService));
        }

        public ActionResult Index()
        {
            return Redirect("https://github.com/NuGet/Home/wiki/Packaging-License-within-the-nupkg");
        }

        public ActionResult DisplayLicense(string licenseExpression)
        {
            var issueId = Guid.NewGuid();
            ViewBag.IssueId = issueId;

            using (_logger.BeginScope("{IssueId}", issueId))
            {
                NuGetLicenseExpression licenseExpressionRootNode;

                if (licenseExpression == null || licenseExpression.Length > 500)
                {
                    return InvalidRequest();
                }

                try
                {
                    licenseExpressionRootNode = NuGetLicenseExpression.Parse(licenseExpression);
                }
                catch (NuGetLicenseExpressionParsingException e)
                {
                    _logger.LogError(0, e, "Got exception while attempting to parse license expression {LicenseExpression}", licenseExpression);
                    return InvalidRequest($"Unable to parse the license expression: {licenseExpression}");
                }

                if (licenseExpressionRootNode.Type == LicenseExpressionType.License)
                {
                    // root of the message can only be license if it's a simple license expression.
                    // or it's a license expression consisting of a single license with +
                    var license = licenseExpressionRootNode as NuGetLicense;

                    if (license == null)
                    {
                        _logger.LogError("Unexpectedly unable to cast NuGetLicenseExpression to NuGetLicense when Type == License while processing {LicenseExpression}",
                            licenseExpression);
                        return InvalidRequest();
                    }

                    if (!license.IsStandardLicense)
                    {
                        return UnknownLicense(license);
                    }

                    return DisplayLicense(license);
                }
                else if (licenseExpressionRootNode.Type == LicenseExpressionType.Operator)
                {
                    var complexLicenseExpressionRoot = licenseExpressionRootNode as LicenseOperator;

                    if (complexLicenseExpressionRoot == null)
                    {
                        _logger.LogError("Unexpectedly unable to cast NuGetLicenseExpression to LicenseOperator wgeb Type == Operator while processing {LicenseExpression}",
                            licenseExpression);
                        return InvalidRequest();
                    }

                    return DisplayComplexLicenseExpression(complexLicenseExpressionRoot, licenseExpression);
                }

                _logger.LogError("Unexpected license expression tree root type: {LicenseExpressionTreeRootType} for expression {LicenseExpression}",
                    licenseExpressionRootNode.Type,
                    licenseExpression);
                return InvalidRequest();
            }
        }

        private ActionResult InvalidRequest(string errorText = null)
        {
            var model = new InvalidRequestModel(errorText);
            Response.StatusCode = 400;
            return View("InvalidRequest", model);
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
                _logger.LogError("license can not be null", nameof(license));
                return InvalidRequest();
            }

            try
            {
                
                string licenseFilePath = _licenseFileService.GetLicenseFilePath(license.Identifier);
                if (!_licenseFileService.DoesLicenseFileExist(licenseFilePath, license.Identifier))
                {
                    return UnknownLicense(license);
                }

                string licenseContent = _licenseFileService.GetLicenseFileContent(licenseFilePath, license.Identifier);
                return View(new SingleLicenseInformationModel(license.Identifier, licenseContent));
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex.Message);
                return InvalidRequest(String.Format("Invalid license identifier: {0}", license.Identifier));
            }
        }

        private ActionResult DisplayComplexLicenseExpression(LicenseOperator licenseExpressionRoot, string licenseExpression)
        {
            var runs = _licenseExpressionSplitter.GetLicenseExpressionRuns(licenseExpressionRoot);
            var fullRuns = _licenseExpressionSplitter.SplitFullExpression(licenseExpression, runs);

            return View("ComplexLicenseExpression", new ComplexLicenseExpressionViewModel(fullRuns));
        }
    }
}