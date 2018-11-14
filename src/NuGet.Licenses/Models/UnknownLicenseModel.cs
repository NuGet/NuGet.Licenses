// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace NuGet.Licenses.Models
{
    public class UnknownLicenseModel
    {
        public UnknownLicenseModel(string licenseName)
        {
            LicenseName = licenseName ?? throw new ArgumentNullException(nameof(licenseName));
        }

        public string LicenseName { get; }
    }
}