// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Licenses
{
    public class LicenseInfo
    {
        public bool IsException { get; set; }
        public string Name { get; set; }
        public string Html { get; set; }
        public string HeaderHtml { get; set; }
        public string Comments { get; set; }
    }
}