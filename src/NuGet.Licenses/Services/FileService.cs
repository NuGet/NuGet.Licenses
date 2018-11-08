// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;

namespace NuGet.Licenses
{
    public class FileService : IFileService
    {
        public bool DoesFileExist(string path)
        {
            return File.Exists(path);
        }

        public string ReadFileContent(string path)
        {
            return File.ReadAllText(path);
        }

        public string GetFileFullPath(string path)
        {
            return Path.GetFullPath(path);
        }
    }
}