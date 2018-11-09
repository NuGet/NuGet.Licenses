// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Moq;
using System;
using System.IO;
using Xunit;

namespace NuGet.Licenses.Tests
{
    public class LicenseFileServiceFacts
    {
        private static string TestLicenseIdentifier = "MIT";
        private static string TestLicenseContent = "MIT license content";
        private static string TestLicensesFolderPath = "TestFolder\\TestPath\\";

        private static ILicenseFileService CreateService(
            Mock<ILicensesFolderPathService> licensesFolderPathService = null,
            Mock<IFileService> fileService = null)
        {
            if (licensesFolderPathService == null)
            {
                licensesFolderPathService = new Mock<ILicensesFolderPathService>();
                licensesFolderPathService
                    .Setup(x => x.GetLicensesFolderPath())
                    .Returns(TestLicensesFolderPath);
            }

            if (fileService == null)
            {
                fileService = new Mock<IFileService>();
                fileService
                    .Setup(x => x.GetFileFullPath(It.IsAny<string>()))
                    .Returns(Path.Combine(TestLicensesFolderPath, TestLicenseIdentifier, ".txt"));
                fileService
                    .Setup(x => x.DoesFileExist(It.IsAny<string>()))
                    .Returns(true);
                fileService
                    .Setup(x => x.ReadFileContent(It.IsAny<string>()))
                    .Returns(TestLicenseContent);
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
        }

        [Fact]
        public void CheckLicenseFileNotExists()
        {
            // Arrange
            var licenseIdentifier = "Test-license";
            var mockFileService = new Mock<IFileService>();
            mockFileService
                .Setup(x => x.GetFileFullPath(It.IsAny<string>()))
                .Returns(Path.Combine(TestLicensesFolderPath, licenseIdentifier, ".txt"));
            mockFileService
                .Setup(x => x.DoesFileExist(It.IsAny<string>()))
                .Returns(false);

            var newService = CreateService(fileService: mockFileService);

            // Act
            var doesFileExist = newService.DoesLicenseFileExist(licenseIdentifier);

            // Assert
            Assert.False(doesFileExist);
        }

        [Fact]
        public void CheckLicenseFileNotAllowed()
        {
            // Arrange
            var licenseIdentifier = "..\\";
            var mockFileService = new Mock<IFileService>();
            mockFileService
                .Setup(x => x.GetFileFullPath(It.IsAny<string>()))
                .Returns("TestFolder\\");

            var newService = CreateService(fileService: mockFileService);

            // Act and Assert
            var exception = Assert.Throws<ArgumentException>(
                () => newService.DoesLicenseFileExist(licenseIdentifier));

            Assert.Equal(nameof(licenseIdentifier), exception.ParamName);
        }

        [Fact]
        public void CheckGetAllowedLicenseFileContent()
        {
            // Arrange
            var newService = CreateService();

            // Act
            var fileContent = newService.GetLicenseFileContent(TestLicenseIdentifier);
            
            // Assert
            Assert.Equal(TestLicenseContent, fileContent);
        }

        [Fact]
        public void CheckGetNotAllowedLicenseFileContent()
        {
            // Arrange
            var licenseIdentifier = "..\\";
            var mockFileService = new Mock<IFileService>();
            mockFileService
                .Setup(x => x.GetFileFullPath(It.IsAny<string>()))
                .Returns("TestFolder\\");

            var newService = CreateService(fileService: mockFileService);

            // Act and Assert
            var exception = Assert.Throws<ArgumentException>(
                () => newService.GetLicenseFileContent(licenseIdentifier));

            Assert.Equal(nameof(licenseIdentifier), exception.ParamName);
        }
    }
}
