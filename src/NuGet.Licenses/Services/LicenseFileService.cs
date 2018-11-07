// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Web;
namespace NuGet.Licenses
{
    public class LicenseFileService : ILicenseFileService
    {
        public string GetLicenseFilePath(string licenseIdentifier)
        {
            return Path.GetFullPath(Path.Combine(HttpContext.Current.Server.MapPath("~\\App_Data\\Licenses\\"), String.Concat(licenseIdentifier, ".txt")));
        }

        public bool IsLicenseFilePathAllowed(string licenseFilePath)
        {
            return licenseFilePath.StartsWith(HttpContext.Current.Server.MapPath("~\\App_Data\\Licenses\\"));
        }

        public bool IsLicenseFileExisted(string licenseFilePath)
        {
            return File.Exists(licenseFilePath);
        }

        public string GetLicenseFileContent(string licenseFilePath)
        {
            return File.ReadAllText(licenseFilePath);
        }
    }
}