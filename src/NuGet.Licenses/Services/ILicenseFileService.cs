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
        /// The function is used to get the full path of the license file.
        /// </summary>
        /// <param name="licenseIdentifier">The license name</param>
        string GetLicenseFilePath(string licenseIdentifier);

        /// <summary>
        /// The function is used to check whether the license file path is allowed.
        /// </summary>
        /// <param name="licenseFilePath">The license file path</param>
        bool IsLicenseFilePathAllowed(string licenseFilePath);

        /// <summary>
        /// The function is used to check whether the license file path is existed.
        /// </summary>
        /// <param name="licenseFilePath">The license file path</param>
        bool IsLicenseFileExisted(string licenseFilePath);
        
        /// <summary>
        /// The function is used to get the license content given the license file path. 
        /// </summary>
        /// <param name="licenseFilePath">The license file path</param>
        string GetLicenseFileContent(string licenseFilePath);
    }
}
