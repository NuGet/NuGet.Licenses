// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Licenses.Models
{
    /// <summary>
    /// Used to specify type of the Run for complex license expression display
    /// </summary>
    public enum ComplexLicenseExpressionRunType
    {
        /// <summary>
        /// Catch-all type for all runs that don't have otherwise specific type
        /// (used mostly for parentheses)
        /// </summary>
        Other = 0,

        /// <summary>
        /// License identifier
        /// </summary>
        LicenseIdentifier = 1,

        /// <summary>
        /// License exception identifier
        /// </summary>
        ExceptionIdentifier = 2,

        /// <summary>
        /// License expression operator
        /// </summary>
        Operator = 3
    }
}