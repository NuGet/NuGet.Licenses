// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Moq;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Xml.Linq;
using Xunit;

namespace NuGet.Licenses.Tests
{
    public class LicenseFileServiceFacts
    {
        private static string TestLicenseIdentifier = "MIT";
        private static LicenseInfo TestLicenseInfo = new LicenseInfo
        {
            IsException = false,
            Name = "Test License",
            Html = "<b>This license is so legit.</b>",
            HeaderHtml = (string)null,
            Comments = "Well, maybe it isn't really.",
        };
        private static string TestLicensesFolderPath = "TestFolder\\TestPath\\";
        private static string TestLicenseFullPath = Path.Combine(TestLicensesFolderPath, String.Concat(TestLicenseIdentifier, ".json"));

        private Mock<ILicensesFolderPathService> _licensesFolderPathService;
        private Mock<IFileService> _fileService;

        public LicenseFileServiceFacts()
        {
            _licensesFolderPathService = new Mock<ILicensesFolderPathService>();
            _licensesFolderPathService
                .Setup(x => x.GetLicensesFolderPath())
                .Returns(TestLicensesFolderPath);

            _fileService = new Mock<IFileService>();
            _fileService
                .Setup(x => x.GetFileFullPath(It.IsAny<string>()))
                .Returns(TestLicenseFullPath);
            _fileService
                .Setup(x => x.DoesFileExist(It.IsAny<string>()))
                .Returns(true);
            _fileService
                .Setup(x => x.ReadFileContent(It.IsAny<string>()))
                .Returns(() => JsonConvert.SerializeObject(TestLicenseInfo));
        }

        private ILicenseFileService CreateService(
            Mock<ILicensesFolderPathService> licensesFolderPathService = null,
            Mock<IFileService> fileService = null)
        {
            if (licensesFolderPathService == null)
            {
                licensesFolderPathService = _licensesFolderPathService;
            }

            if (fileService == null)
            {
                fileService = _fileService;
            }

            return new LicenseFileService(licensesFolderPathService.Object, fileService.Object);
        }

        [Fact]
        public void CheckLicenseFileExists()
        {
            // Arrange
            var newService = CreateService();

            // Act
            var doesFileExist = newService.DoesLicenseFileExist(TestLicenseIdentifier);

            // Assert
            Assert.True(doesFileExist);
            _licensesFolderPathService.Verify(
                x => x.GetLicensesFolderPath(),
                Times.Exactly(2));
            _fileService.Verify(
                x => x.DoesFileExist(TestLicenseFullPath),
                Times.Once);
            _fileService.Verify(
                x => x.GetFileFullPath(TestLicenseFullPath),
                Times.Once);
        }

        [Fact]
        public void CheckLicenseFileNotExists()
        {
            // Arrange
            var licenseIdentifier = "Test-license";
            var licenseFullPath = Path.Combine(TestLicensesFolderPath, String.Concat(licenseIdentifier, ".json"));
            var mockFileService = new Mock<IFileService>();
            mockFileService
                .Setup(x => x.GetFileFullPath(It.IsAny<string>()))
                .Returns(licenseFullPath);
            mockFileService
                .Setup(x => x.DoesFileExist(It.IsAny<string>()))
                .Returns(false);

            var newService = CreateService(fileService: mockFileService);

            // Act
            var doesFileExist = newService.DoesLicenseFileExist(licenseIdentifier);

            // Assert
            Assert.False(doesFileExist);
            _licensesFolderPathService.Verify(
                x => x.GetLicensesFolderPath(),
                Times.Exactly(2));
            mockFileService.Verify(
                x => x.DoesFileExist(licenseFullPath),
                Times.Once);
            mockFileService.Verify(
                x => x.GetFileFullPath(licenseFullPath),
                Times.Once);
        }

        [Fact]
        public void CheckLicenseFileNotAllowed()
        {
            // Arrange
            var licenseIdentifier = "..\\";
            var licenseFullPath = Path.Combine(TestLicensesFolderPath, String.Concat(licenseIdentifier, ".json"));
            var mockFileService = new Mock<IFileService>();
            mockFileService
                .Setup(x => x.GetFileFullPath(It.IsAny<string>()))
                .Returns("TestFolder\\");

            var newService = CreateService(fileService: mockFileService);

            // Act and Assert
            var exception = Assert.Throws<ArgumentException>(
                () => newService.DoesLicenseFileExist(licenseIdentifier));

            Assert.Equal(nameof(licenseIdentifier), exception.ParamName);
            _licensesFolderPathService.Verify(
                x => x.GetLicensesFolderPath(),
                Times.Exactly(2));
            mockFileService.Verify(
                x => x.GetFileFullPath(licenseFullPath),
                Times.Once);
            mockFileService.Verify(
                x => x.DoesFileExist(It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public void CheckGetAllowedLicenseFileContent()
        {
            // Arrange
            var newService = CreateService();

            // Act
            var info = newService.GetLicenseInfo(TestLicenseIdentifier);
            
            // Assert
            Assert.Equal(TestLicenseInfo.Name, info.Name);
            Assert.Equal(TestLicenseInfo.Comments, info.Comments);
            Assert.Equal(TestLicenseInfo.Html, info.Html);
            Assert.Equal(TestLicenseInfo.HeaderHtml, info.HeaderHtml);
            Assert.Equal(TestLicenseInfo.IsException, info.IsException);
            _licensesFolderPathService.Verify(
                x => x.GetLicensesFolderPath(),
                Times.Exactly(2));
            _fileService.Verify(
                x => x.GetFileFullPath(TestLicenseFullPath),
                Times.Once);
            _fileService.Verify(
                x => x.ReadFileContent(TestLicenseFullPath),
                Times.Once);
        }

        [Fact]
        public void CheckGetNotAllowedLicenseFileContent()
        {
            // Arrange
            var licenseIdentifier = "..\\";
            var licenseFullPath = Path.Combine(TestLicensesFolderPath, String.Concat(licenseIdentifier, ".json"));
            var mockFileService = new Mock<IFileService>();
            mockFileService
                .Setup(x => x.GetFileFullPath(It.IsAny<string>()))
                .Returns("TestFolder\\");

            var newService = CreateService(fileService: mockFileService);

            // Act and Assert
            var exception = Assert.Throws<ArgumentException>(
                () => newService.GetLicenseInfo(licenseIdentifier));

            Assert.Equal(nameof(licenseIdentifier), exception.ParamName);
            _licensesFolderPathService.Verify(
                x => x.GetLicensesFolderPath(),
                Times.Exactly(2));
            mockFileService.Verify(
                x => x.GetFileFullPath(licenseFullPath),
                Times.Once);
            mockFileService.Verify(
                x => x.ReadFileContent(It.IsAny<string>()),
                Times.Never);
        }
    }
}
