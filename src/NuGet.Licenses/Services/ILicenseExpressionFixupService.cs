// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Licenses.Services
{
    /// <summary>
    /// Interface for a service that does initial pre-processing of a license expression.
    /// </summary>
    public interface ILicenseExpressionFixupService
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
        /// 
        /// When there are not spaces present, the decoding results are the same
        /// between client encoding and "proper" encoding.
        /// The cases when spaces matter are the cases when composite license
        /// expression is used, i.e.OR, AND or WITH operators are used and
        /// since spec prescribes them to have spaces on both sides, we'll just
        /// check for the presence of "+OR+", "+AND+" or "+WITH+" substrings
        /// and decode differently based on that.
        /// </remarks>
        /// <param name="undecodedLicenseExpression">License expression as we received it in HTTP request without any URL decoding done.</param>
        /// <returns>License expression ready to feed to the parser.</returns>
        string FixupLicenseExpression(string undecodedLicenseExpression);
    }
}
