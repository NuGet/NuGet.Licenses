// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net;

namespace NuGet.Licenses.Services
{
    public class LicenseExpressionDecodingService : ILicenseExpressionDecodingService
    {
        /// <remarks>
        /// When there are not spaces present, the decoding results are the same
        /// between 15.9 client encoding and "proper" encoding.
        /// The cases when spaces matter are the cases when composite license
        /// expression is used, i.e. OR, AND or WITH operators are used and
        /// since spec prescribes them to have spaces on both sides, we'll just
        /// check for the presence of "+OR+", "+AND+" or "+WITH+" substrings
        /// and decode differently based on that.
        /// </remarks>
        public string DecodeLicenseExpression(string encodedLicenseExpression)
        {
            if (encodedLicenseExpression == null)
            {
                throw new ArgumentNullException(nameof(encodedLicenseExpression));
            }

            var client15_9EncodingIndicators = new string[] { "+OR+", "+AND+", "+WITH+" };

            var encodedByClient15_9 = client15_9EncodingIndicators.Any(indicator => encodedLicenseExpression.Contains(indicator));

            if (encodedByClient15_9)
            {
                return Decode15_9ClientLicenseExpression(encodedLicenseExpression);
            }

            return DecodeRegularLicenseExpression(encodedLicenseExpression);
        }

        private string Decode15_9ClientLicenseExpression(string encodedLicenseExpression)
        {
            // client uses WebUtility.UrlEncode, so we'll use its decoding
            // counterpart to process the string
            return WebUtility.UrlDecode(encodedLicenseExpression);

            // The following also works if we don't want to do full decoding:
            //return undecodedLicenseExpression.Replace("+", " ").Replace("%2B", "+");
        }

        private string DecodeRegularLicenseExpression(string encodedLicenseExpression)
        {
            return Uri.UnescapeDataString(encodedLicenseExpression);

            // The following also works if we don't want to do full decoding:
            //return undecodedLicenseExpression.Replace("%20", " ").Replace("%2B", "+");
        }
    }
}