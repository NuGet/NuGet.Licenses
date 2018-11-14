// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Licenses
{
    /// <summary>
    /// This interface is used to get the path of the licenses folder.
    /// /// </summary>
    public interface ILicensesFolderPathService
    {
        /// <summary>
        /// The function is used to get the path of the licenses folder.
        /// </summary>
        string GetLicensesFolderPath();
    }
}
