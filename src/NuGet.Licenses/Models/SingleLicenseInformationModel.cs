// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace NuGet.Licenses.Models
{
    public class SingleLicenseInformationModel
    {
        public SingleLicenseInformationModel(string identifier, bool isException, LicenseInfo info, DateTimeOffset lastUpdated)
        {
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            Info = info ?? throw new ArgumentNullException(nameof(info));
            IsException = isException;
            LastUpdated = lastUpdated;
        }

        public string Identifier { get; }
        public LicenseInfo Info { get; }
        public bool IsException { get; }
        public DateTimeOffset LastUpdated { get; }
    }
}