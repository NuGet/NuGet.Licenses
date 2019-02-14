// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web;
using System.Web.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NuGet.Licenses.Controllers;
using NuGet.Licenses.Models;
using NuGet.Licenses.Services;
using Xunit;

namespace NuGet.Licenses.Tests
{
    public class LicenseControllerFacts
    {
        [Fact]
        public void IndexRedirects()
        {
            var controller = CreateController();
            ActionResult result = null;
            var ex = Record.Exception(() => result = controller.Index());
            Assert.Null(ex);
            var redirect = Assert.IsType<RedirectResult>(result);
            Assert.Equal("https://aka.ms/licenses.nuget.org", redirect.Url);
        }

        [Fact]
        public void HandlesUnknownLicenseProperly()
        {
            var controller = CreateController();

            ActionResult result = null;
            const string unknownLicenseName = "TestUnknownLicense";
            var ex = Record.Exception(() => result = controller.DisplayLicense(unknownLicenseName));
            Assert.Null(ex);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UnknownLicenseModel>(viewResult.Model);
            Assert.Equal(unknownLicenseName, model.LicenseName);
            Assert.Equal("UnknownLicense", viewResult.ViewName);
        }

        [Fact]
        public void HandlesUnparseableExpressionProperly()
        {
            var controller = CreateController();

            ActionResult result = null;
            const string unparseableLicenseExpression = "MIT OR";
            var ex = Record.Exception(() => result = controller.DisplayLicense(unparseableLicenseExpression));
            Assert.Null(ex);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<InvalidRequestModel>(viewResult.Model);
            Assert.Contains(unparseableLicenseExpression, model.Message);
            Assert.Equal("InvalidRequest", viewResult.ViewName);
        }

        private LicenseController CreateController(
            ILicenseExpressionSegmentator segmentator = null,
            ILogger<LicenseController> logger = null,
            ILicenseFileService licenseFileService = null)
        {
            if (segmentator == null)
            {
                segmentator = Mock.Of<ILicenseExpressionSegmentator>();
            }

            if (logger == null)
            {
                logger = Mock.Of<ILogger<LicenseController>>();
            }

            if (licenseFileService == null)
            {
                licenseFileService = Mock.Of<ILicenseFileService>();
            }

            var controller = new LicenseController(segmentator, logger, licenseFileService);
            var httpContextMock = new Mock<HttpContextBase>();
            httpContextMock
                .SetupGet(ctx => ctx.Response)
                .Returns(Mock.Of<HttpResponseBase>());
            var controllerContext = new ControllerContext();
            controllerContext.HttpContext = httpContextMock.Object;
            controller.ControllerContext = controllerContext;

            return controller;
        }
    }
}
