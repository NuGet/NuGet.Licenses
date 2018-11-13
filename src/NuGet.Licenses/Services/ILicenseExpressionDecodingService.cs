// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Licenses.Services
{
    /// <summary>
    /// Interface for a service that does initial pre-processing of a license expression.
    /// </summary>
    public interface ILicenseExpressionDecodingService
    {
        /// <summary>
        /// Detects and fixes the URL encoding issues that might be present
        /// in license expression.
        /// </summary>
        /// <remarks>
        /// nuget.exe released with VS 15.9 encodes URLs in a funny way:
        /// MIT+ OR Apache-2.0 gets encoded as "MIT%2B+OR+Apache-2.0"
        /// which does not decode well using standard decoder(it gets
        /// decoded to "MIT++OR+Apache-2.0")
        /// 
        /// So, we set up IIS to pass undecoded strings to us and here we'll
        /// try to detect how license expression was encoded and try to decode
        /// it properly.
        /// </remarks>
        /// <param name="encodedLicenseExpression">License expression as we received it in HTTP request without any URL decoding done.</param>
        /// <returns>License expression ready to feed to the parser.</returns>
        string DecodeLicenseExpression(string encodedLicenseExpression);
    }
}
