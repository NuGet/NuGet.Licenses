// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Web;

namespace NuGet.Licenses
{
    public class LicensesFolderPathService : ILicensesFolderPathService
    {
        private const string licensesFolderPath = "~\\App_Data\\Licenses\\";
        public string getLicensesFolderPath(){
            return HttpContext.Current.Server.MapPath(licensesFolderPath);
        }
    }
}