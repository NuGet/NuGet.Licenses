// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Licenses
{
    /// <summary>
    /// This interface is used to provide services for license files.   
    /// </summary>
    public interface ILicenseFileService
    {
        /// <summary>
        /// The function is used to check whether the license file path exists.
        /// </summary>
        /// <param name="licenseIdentifier">The license name</param>
        bool DoesLicenseFileExist(string licenseIdentifier);

        /// <summary>
        /// The function is used to get the license content given the license file path. 
        /// </summary>
        /// <param name="licenseIdentifier">The license name</param>
        string GetLicenseFileContent(string licenseIdentifier);
    }
}
