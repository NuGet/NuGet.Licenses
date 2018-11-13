// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace NuGet.Licenses.Models
{
    public class CompositeLicenseExpressionViewModel
    {
        public CompositeLicenseExpressionViewModel(IReadOnlyCollection<CompositeLicenseExpressionSegment> segments)
        {
            Segments = segments ?? throw new ArgumentNullException(nameof(segments));
        }

        public IReadOnlyCollection<CompositeLicenseExpressionSegment> Segments { get; }
    }
}