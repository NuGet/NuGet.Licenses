// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Web.Mvc;
using Microsoft.Extensions.Logging;
using NuGet.Licenses.Models;
using NuGet.Licenses.Services;
using NuGet.Packaging.Licenses;

namespace NuGet.Licenses.Controllers
{
    public class LicenseController : Controller
    {
        private readonly ILicenseExpressionSegmentator _licenseExpressionSegmentator;
        private readonly ILogger<LicenseController> _logger;
        private readonly ILicenseFileService _licenseFileService;
        private readonly ILicenseExpressionDecodingService _licenseExpressionDecodingService;

        public LicenseController(
            ILicenseExpressionSegmentator licenseExpressionSegmentator,
            ILogger<LicenseController> logger,
            ILicenseFileService licenseFileService,
            ILicenseExpressionDecodingService licenseExpressionDecodingService)
        {
            _licenseExpressionSegmentator = licenseExpressionSegmentator ?? throw new ArgumentNullException(nameof(licenseExpressionSegmentator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _licenseFileService = licenseFileService ?? throw new ArgumentNullException(nameof(licenseFileService));
            _licenseExpressionDecodingService = licenseExpressionDecodingService ?? throw new ArgumentNullException(nameof(licenseExpressionDecodingService));
        }

        public ActionResult Index()
        {
            return Redirect("https://aka.ms/licenses.nuget.org");
        }

        public ActionResult DisplayLicense(string licenseExpression)
        {
            var issueId = Guid.NewGuid();
            ViewBag.IssueId = Activity.Current?.Id;

            var processedLicenseExpression = _licenseExpressionDecodingService.DecodeLicenseExpression(licenseExpression);

            if (NuGetLicenseData.ExceptionList.TryGetValue(processedLicenseExpression, out var exceptionData))
            {
                return DisplayException(exceptionData);
            }

            if (processedLicenseExpression == null || processedLicenseExpression.Length > 500)
            {
                return InvalidRequest();
            }

            NuGetLicenseExpression licenseExpressionRootNode;

            try
            {
                licenseExpressionRootNode = NuGetLicenseExpression.Parse(processedLicenseExpression);
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
                var compositeLicenseExpressionRoot = licenseExpressionRootNode as LicenseOperator;

                if (compositeLicenseExpressionRoot == null)
                {
                    _logger.LogError("Unexpectedly unable to cast NuGetLicenseExpression to LicenseOperator with Type == Operator while processing {LicenseExpression}",
                        licenseExpression);
                    return InvalidRequest();
                }

                return DisplayCompositeLicenseExpression(compositeLicenseExpressionRoot, processedLicenseExpression);
            }

            _logger.LogError("Unexpected license expression tree root type: {LicenseExpressionTreeRootType} for expression {LicenseExpression}",
                licenseExpressionRootNode.Type,
                licenseExpression);
            return InvalidRequest();
        }

        private ActionResult InvalidRequest(string errorText = null)
        {
            var model = new InvalidRequestModel(errorText);
            Response.StatusCode = 400;
            return View("InvalidRequest", model);
        }

        private ActionResult UnknownLicense(NuGetLicense license)
        {
            return UnknownLicense(license);
        }

        private ActionResult UnknownLicense(string licenseIdentifier)
        {
            Response.StatusCode = 404;
            return View("UnknownLicense", new UnknownLicenseModel(licenseIdentifier));
        }

        private ActionResult DisplayLicense(NuGetLicense license)
        {
            return DisplayLicenseFromFile(license.Identifier, isException: false);
        }

        private ActionResult DisplayException(ExceptionData exception)
        {
            return DisplayLicenseFromFile(exception.LicenseExceptionID, isException: true);
        }

        private ActionResult DisplayLicenseFromFile(string identifier, bool isException)
        {
            try
            {
                if (!_licenseFileService.DoesLicenseFileExist(identifier))
                {
                    return UnknownLicense(identifier);
                }

                string licenseContent = _licenseFileService.GetLicenseFileContent(identifier);
                return View("DisplayLicense", new SingleLicenseInformationModel(identifier, licenseContent, isException));
            }
            catch (ArgumentException e)
            {
                _logger.LogError(0, e, "Got exception while attempting to get license contents due to the invalid license: {licenseIdentifier}", identifier);
                return InvalidRequest();
            }
        }

        private ActionResult DisplayCompositeLicenseExpression(LicenseOperator licenseExpressionRoot, string licenseExpression)
        {
            var meaningfulSegments = _licenseExpressionSegmentator.GetLicenseExpressionSegments(licenseExpressionRoot);
            var allSegments = _licenseExpressionSegmentator.SplitFullExpression(licenseExpression, meaningfulSegments);

            return View("CompositeLicenseExpression", new CompositeLicenseExpressionViewModel(allSegments));
        }
    }
}