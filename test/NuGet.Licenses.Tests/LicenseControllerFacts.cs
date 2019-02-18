// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;
using Moq;
using NuGet.Licenses.Controllers;
using NuGet.Licenses.Models;
using NuGet.Licenses.Services;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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

        [Theory]
        [MemberData(nameof(GetValidCompositeLicenseExpression))]
        public void HandlesValidCompositeLicenseExpression(string licenseExpression,
                                                           List<CompositeLicenseExpressionSegment> allSegments)
        {
            // Arrange
            var segmentatorService = new Mock<ILicenseExpressionSegmentator>();
            segmentatorService
                .Setup(x => x.SplitFullExpression(licenseExpression, It.IsAny<List<CompositeLicenseExpressionSegment>>()))
                .Returns(allSegments);
            var licenseFileService = new Mock<ILicenseFileService>();
            licenseFileService
                .Setup(x => x.DoesLicenseFileExist(It.IsAny<string>()))
                .Returns(true);

            var controller = CreateController(segmentator: segmentatorService, licenseFileService: licenseFileService);

            // Act
            var result = controller.DisplayLicense(licenseExpression);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CompositeLicenseExpressionViewModel>(viewResult.Model);
            var segments = model.Segments;
            Assert.Equal(allSegments.Count, segments.Count);
            for (var i = 0; i < segments.Count; i++)
            {
                Assert.Equal(allSegments.ElementAt(i).Type, segments.ElementAt(i).Type);
                Assert.Equal(allSegments.ElementAt(i).Value, segments.ElementAt(i).Value);
            }
        }

        public static IEnumerable<object[]> GetValidCompositeLicenseExpression
        {
            get
            {
                yield return new object[] {
                    "MIT AND AAL",
                    new List<CompositeLicenseExpressionSegment>
                    {
                        new CompositeLicenseExpressionSegment("MIT", CompositeLicenseExpressionSegmentType.LicenseIdentifier),
                        new CompositeLicenseExpressionSegment(" ", CompositeLicenseExpressionSegmentType.Other),
                        new CompositeLicenseExpressionSegment("AND", CompositeLicenseExpressionSegmentType.Operator),
                        new CompositeLicenseExpressionSegment(" ", CompositeLicenseExpressionSegmentType.Other),
                        new CompositeLicenseExpressionSegment("AAL", CompositeLicenseExpressionSegmentType.LicenseIdentifier)
                    }
                };
                yield return new object[] {
                    "MIT WITH Bison-exception-2.2",
                    new List<CompositeLicenseExpressionSegment>
                    {
                        new CompositeLicenseExpressionSegment("MIT", CompositeLicenseExpressionSegmentType.LicenseIdentifier),
                        new CompositeLicenseExpressionSegment(" ", CompositeLicenseExpressionSegmentType.Other),
                        new CompositeLicenseExpressionSegment("WITH", CompositeLicenseExpressionSegmentType.Operator),
                        new CompositeLicenseExpressionSegment(" ", CompositeLicenseExpressionSegmentType.Other),
                        new CompositeLicenseExpressionSegment("Bison-exception-2.2", CompositeLicenseExpressionSegmentType.ExceptionIdentifier)
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(GetInValidCompositeLicenseExpression))]
        public void HandlesInValidCompositeLicenseExpression(string licenseExpression,
                                                             string invalidLicenseName,
                                                             List<CompositeLicenseExpressionSegment> allSegments)
        {
            // Arrange
            var segmentatorService = new Mock<ILicenseExpressionSegmentator>();
            segmentatorService
                .Setup(x => x.SplitFullExpression(licenseExpression, It.IsAny<List<CompositeLicenseExpressionSegment>>()))
                .Returns(allSegments);
            var licenseFileService = new Mock<ILicenseFileService>();
            licenseFileService
                .Setup(x => x.DoesLicenseFileExist(It.IsAny<string>()))
                .Returns(true);

            var controller = CreateController(segmentator: segmentatorService, licenseFileService: licenseFileService);

            // Act
            var result = controller.DisplayLicense(licenseExpression);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UnknownLicenseModel>(viewResult.Model);
            Assert.Equal(invalidLicenseName, model.LicenseName);
            Assert.Equal("UnknownLicense", viewResult.ViewName);
        }

        public static IEnumerable<object[]> GetInValidCompositeLicenseExpression
        {
            get
            {
                yield return new object[] {
                    "MIT AND UnknownLicense",
                    "UnknownLicense",
                    new List<CompositeLicenseExpressionSegment>
                    {
                        new CompositeLicenseExpressionSegment("MIT", CompositeLicenseExpressionSegmentType.LicenseIdentifier),
                        new CompositeLicenseExpressionSegment(" ", CompositeLicenseExpressionSegmentType.Other),
                        new CompositeLicenseExpressionSegment("AND", CompositeLicenseExpressionSegmentType.Operator),
                        new CompositeLicenseExpressionSegment(" ", CompositeLicenseExpressionSegmentType.Other),
                        new CompositeLicenseExpressionSegment("UnknownLicense", CompositeLicenseExpressionSegmentType.LicenseIdentifier)
                    }
                };
                yield return new object[] {
                    "UnknownLicense AND MIT",
                    "UnknownLicense",
                    new List<CompositeLicenseExpressionSegment>
                    {
                        new CompositeLicenseExpressionSegment("UnknownLicense", CompositeLicenseExpressionSegmentType.LicenseIdentifier),
                        new CompositeLicenseExpressionSegment(" ", CompositeLicenseExpressionSegmentType.Other),
                        new CompositeLicenseExpressionSegment("AND", CompositeLicenseExpressionSegmentType.Operator),
                        new CompositeLicenseExpressionSegment(" ", CompositeLicenseExpressionSegmentType.Other),
                        new CompositeLicenseExpressionSegment("MIT", CompositeLicenseExpressionSegmentType.LicenseIdentifier)
                    }
                };
                yield return new object[] {
                    "MIT AND UnknownLicense AND UnknownLicense2",
                    "UnknownLicense",
                    new List<CompositeLicenseExpressionSegment>
                    {
                        new CompositeLicenseExpressionSegment("MIT", CompositeLicenseExpressionSegmentType.LicenseIdentifier),
                        new CompositeLicenseExpressionSegment(" ", CompositeLicenseExpressionSegmentType.Other),
                        new CompositeLicenseExpressionSegment("AND", CompositeLicenseExpressionSegmentType.Operator),
                        new CompositeLicenseExpressionSegment(" ", CompositeLicenseExpressionSegmentType.Other),
                        new CompositeLicenseExpressionSegment("UnknownLicense", CompositeLicenseExpressionSegmentType.LicenseIdentifier),
                        new CompositeLicenseExpressionSegment(" ", CompositeLicenseExpressionSegmentType.Other),
                        new CompositeLicenseExpressionSegment("AND", CompositeLicenseExpressionSegmentType.Operator),
                        new CompositeLicenseExpressionSegment(" ", CompositeLicenseExpressionSegmentType.Other),
                        new CompositeLicenseExpressionSegment("UnknownLicense2", CompositeLicenseExpressionSegmentType.LicenseIdentifier)
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(GetValidCompositeLicenseExpressionButNoLicenseFile))]
        public void HandlesValidCompositeLicenseExpressionButNoLicenseFile(string licenseExpression,
                                                                           string invalidLicenseName,
                                                                           List<CompositeLicenseExpressionSegment> allSegments)
        {
            // Arrange
            var segmentatorService = new Mock<ILicenseExpressionSegmentator>();
            segmentatorService
                .Setup(x => x.SplitFullExpression(licenseExpression, It.IsAny<List<CompositeLicenseExpressionSegment>>()))
                .Returns(allSegments);
            var licenseFileService = new Mock<ILicenseFileService>();
            licenseFileService
                .Setup(x => x.DoesLicenseFileExist(invalidLicenseName))
                .Returns(false);

            var controller = CreateController(segmentator: segmentatorService, licenseFileService: licenseFileService);

            // Act
            var result = controller.DisplayLicense(licenseExpression);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UnknownLicenseModel>(viewResult.Model);
            Assert.Equal(invalidLicenseName, model.LicenseName);
            Assert.Equal("UnknownLicense", viewResult.ViewName);
        }

        public static IEnumerable<object[]> GetValidCompositeLicenseExpressionButNoLicenseFile
        {
            get
            {
                yield return new object[] {
                    "MIT AND AAL",
                    "MIT",
                    new List<CompositeLicenseExpressionSegment>
                    {
                        new CompositeLicenseExpressionSegment("MIT", CompositeLicenseExpressionSegmentType.LicenseIdentifier),
                        new CompositeLicenseExpressionSegment(" ", CompositeLicenseExpressionSegmentType.Other),
                        new CompositeLicenseExpressionSegment("AND", CompositeLicenseExpressionSegmentType.Operator),
                        new CompositeLicenseExpressionSegment(" ", CompositeLicenseExpressionSegmentType.Other),
                        new CompositeLicenseExpressionSegment("AAL", CompositeLicenseExpressionSegmentType.LicenseIdentifier)
                    }
                };
            }
        }

        private LicenseController CreateController(
            Mock<ILicenseExpressionSegmentator> segmentator = null,
            Mock<ILogger<LicenseController>> logger = null,
            Mock<ILicenseFileService> licenseFileService = null)
        {
            if (segmentator == null)
            {
                segmentator = new Mock<ILicenseExpressionSegmentator>();
            }

            if (logger == null)
            {
                logger = new Mock<ILogger<LicenseController>>();
            }

            if (licenseFileService == null)
            {
                licenseFileService = new Mock<ILicenseFileService>();
            }

            var controller = new LicenseController(segmentator.Object, logger.Object, licenseFileService.Object);
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
