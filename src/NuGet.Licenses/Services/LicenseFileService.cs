// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Web;
namespace NuGet.Licenses
{
    public class LicenseFileService : ILicenseFileService
    {
        private readonly ILicensesFolderPathService _licensesFolderPathService;
        public LicenseFileService(ILicensesFolderPathService licensesFolderPathService)
        {
            _licensesFolderPathService = licensesFolderPathService ?? throw new ArgumentNullException(nameof(licensesFolderPathService));
        }

        public string GetLicenseFilePath(string licenseIdentifier)
        {
            string licenseFilePath = Path.GetFullPath(Path.Combine(_licensesFolderPathService.getLicensesFolderPath(), String.Concat(licenseIdentifier, ".txt")));
            if (!IsLicenseFilePathAllowed(licenseFilePath))
            {
                throw new ArgumentException(String.Format("{0} is not a valid license", licenseIdentifier), nameof(licenseIdentifier));
            }

            return licenseFilePath;
        }

        public bool DoesLicenseFileExist(string licenseFilePath, string licenseIdentifier)
        {
            if (!IsLicenseFilePathAllowed(licenseFilePath))
            {
                throw new ArgumentException(String.Format("{0} is not a valid license", licenseIdentifier), nameof(licenseIdentifier));
            }

            return File.Exists(licenseFilePath);
        }

        public string GetLicenseFileContent(string licenseFilePath, string licenseIdentifier)
        {
            if (!IsLicenseFilePathAllowed(licenseFilePath))
            {
                throw new ArgumentException(String.Format("{0} is not a valid license", licenseIdentifier), nameof(licenseIdentifier));
            }

            return File.ReadAllText(licenseFilePath);
        }

        private bool IsLicenseFilePathAllowed(string licenseFilePath)
        {
            return licenseFilePath.StartsWith(_licensesFolderPathService.getLicensesFolderPath());
        }
    }
}