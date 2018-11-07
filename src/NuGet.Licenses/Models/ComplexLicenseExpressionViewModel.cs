// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace NuGet.Licenses.Models
{
    public class ComplexLicenseExpressionViewModel
    {
        public ComplexLicenseExpressionViewModel(IReadOnlyCollection<ComplexLicenseExpressionRun> runs)
        {
            Runs = runs ?? throw new ArgumentNullException(nameof(runs));
        }

        public IReadOnlyCollection<ComplexLicenseExpressionRun> Runs { get; }
    }
}