// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net;

namespace NuGet.Licenses.Services
{
    public class LicenseExpressionDecodingService : ILicenseExpressionDecodingService
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
                return Decode15_9ClientLicenseExpression(undecodedLicenseExpression);
            }

            return DecodeRegularLicenseExpression(undecodedLicenseExpression);
        }

        private string Decode15_9ClientLicenseExpression(string undecodedLicenseExpression)
        {
            // client uses WebUtility.UrlEncode, so we'll use its decoding
            // counterpart to process the string
            return WebUtility.UrlDecode(undecodedLicenseExpression);

            // The following also works if we don't want to do full decoding:
            //return undecodedLicenseExpression.Replace("+", " ").Replace("%2B", "+");
        }

        private string DecodeRegularLicenseExpression(string undecodedLicenseExpression)
        {
            return Uri.UnescapeDataString(undecodedLicenseExpression);

            // The following also works if we don't want to do full decoding:
            //return undecodedLicenseExpression.Replace("%20", " ").Replace("%2B", "+");
        }
    }
}