// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Licenses
{
    /// <summary>
    /// This interface is used to provide basic services for files.
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// The function is used to check whether the file exists given the path.
        /// </summary>
        /// <param name="path">The file path</param>
        bool DoesFileExist(string path);
        
        /// <summary>
        /// The function is used to read the file content given the path.
        /// </summary>
        /// <param name="path">The file path</param>
        string ReadFileContent(string path);
        
        /// <summary>
        /// The function is used to form the full file path.
        /// </summary>
        /// <param name="path">The file path</param>
        string GetFileFullPath(string path);
    }
}
