// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace NuGet.Licenses
{
    /// <summary>
    /// This interface is used to provide services for license files.   
    /// </summary>
    public interface ILicenseFileService
    {
        /// <summary>
        /// The function is used to check whether the license or exception file path exists.
        /// </summary>
        /// <param name="licenseIdentifier">The license or exception name</param>
        bool DoesLicenseFileExist(string licenseIdentifier);

        /// <summary>
        /// The function is used to get the license or exception content given the license file path. 
        /// </summary>
        /// <param name="licenseIdentifier">The license or exception name</param>
        LicenseInfo GetLicenseInfo(string licenseIdentifier);

        /// <summary>
        /// Gets the timestamp when the license data was last updated from the SPDX source.
        /// </summary>
        DateTimeOffset GetLastUpdated();
    }
}
