// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace NuGet.Licenses.Models
{
    public class UnknownLicenseModel
    {
        public UnknownLicenseModel(string licenseName, DateTimeOffset lastUpdated)
        {
            LicenseName = licenseName ?? throw new ArgumentNullException(nameof(licenseName));
            LastUpdated = lastUpdated;
        }

        public string LicenseName { get; }
        public DateTimeOffset LastUpdated { get; }
    }
}