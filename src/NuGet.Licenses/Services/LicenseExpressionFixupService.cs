// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net;

namespace NuGet.Licenses.Services
{
    public class LicenseExpressionFixupService : ILicenseExpressionFixupService
    {
        public string FixupLicenseExpression(string undecodedLicenseExpression)
        {
            if (undecodedLicenseExpression == null)
            {
                throw new ArgumentNullException(nameof(undecodedLicenseExpression));
            }

            var funnyEncodingIndicators = new string[] { "+OR+", "+AND+", "+WITH+" };

            var funnyEncoded = funnyEncodingIndicators.Any(indicator => undecodedLicenseExpression.Contains(indicator));

            if (funnyEncoded)
            {
                // client uses WebUtility.UrlEncode, so we'll use its decoding
                // counterpart to process the string
                return WebUtility.UrlDecode(undecodedLicenseExpression);
            }

            return Uri.UnescapeDataString(undecodedLicenseExpression);
        }
    }
}