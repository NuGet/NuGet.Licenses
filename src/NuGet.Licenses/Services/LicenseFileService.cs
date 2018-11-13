// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;

namespace NuGet.Licenses
{
    public class LicenseFileService : ILicenseFileService
    {
        private readonly ILicensesFolderPathService _licensesFolderPathService;
        private readonly IFileService _fileService;

        public LicenseFileService(ILicensesFolderPathService licensesFolderPathService, IFileService fileService)
        {
            _licensesFolderPathService = licensesFolderPathService ?? throw new ArgumentNullException(nameof(licensesFolderPathService));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        }

        public bool DoesLicenseFileExist(string licenseIdentifier)
        {
            return _fileService.DoesFileExist(GetLicenseFilePath(licenseIdentifier));
        }

        public string GetLicenseFileContent(string licenseIdentifier)
        {
            return _fileService.ReadFileContent(GetLicenseFilePath(licenseIdentifier));
        }

        private string GetLicenseFilePath(string licenseIdentifier)
        {
            string licenseFilePath = _fileService.GetFileFullPath(Path.Combine(_licensesFolderPathService.GetLicensesFolderPath(), String.Concat(licenseIdentifier, ".txt")));
            if (!IsLicenseFilePathAllowed(licenseFilePath))
            {
                throw new ArgumentException(String.Format("{0} is not a valid license", licenseIdentifier), nameof(licenseIdentifier));
            }

            return licenseFilePath;
        }

        private bool IsLicenseFilePathAllowed(string licenseFilePath)
        {
            return licenseFilePath.StartsWith(_licensesFolderPathService.GetLicensesFolderPath());
        }
    }
}