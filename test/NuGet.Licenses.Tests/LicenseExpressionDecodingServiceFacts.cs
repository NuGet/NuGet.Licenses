// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using NuGet.Licenses.Services;
using Xunit;

namespace NuGet.Licenses.Tests
{
    public class LicenseExpressionDecodingServiceFacts
    {
        [Fact]
        public void ThrowsIfLicenseExpressionIsNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => _target.DecodeLicenseExpression(null));
            Assert.Equal("encodedLicenseExpression", ex.ParamName);
        }

        public static IEnumerable<object[]> LicenseExpressions => new object[][]
        {
            new object[]{ "MIT" },
            new object[]{ "MIT+" },
            new object[]{ "MIT OR Apache-2.0" },
            new object[]{ "MIT+ OR Apache-2.0" },
            new object[]{ "MIT OR (Foo+ AND bar)" },
        };

        [Theory]
        [MemberData(nameof(LicenseExpressions))]
        public void CorrectlyDetectsAndDecodesSpacesAsPlus(string licenseExpression)
        {
            var encoded = WebUtility.UrlEncode(licenseExpression);

            var result = _target.DecodeLicenseExpression(encoded);

            Assert.Equal(licenseExpression, result);
        }

        [Theory]
        [MemberData(nameof(LicenseExpressions))]
        public void CorrectlyDetectsAndDecodesProperlyEncodedExpressions(string licenseExpression)
        {
            var encoded = GetUriEncodedLicenseExpression(licenseExpression);

            var result = _target.DecodeLicenseExpression(encoded);

            Assert.Equal(licenseExpression, result);
        }

        [Theory]
        [MemberData(nameof(LicenseExpressions))]
        public void CorrectlyDetectsAndDecodesProperlyWithUnencodedPluses(string licenseExpression)
        {
            var encoded = GetUriEncodedLicenseExpression(licenseExpression);
            encoded = encoded.Replace("%2B", "+");

            var result = _target.DecodeLicenseExpression(encoded);

            Assert.Equal(licenseExpression, result);
        }

        private string GetUriEncodedLicenseExpression(string licenseExpression)
        {
            var uri = new Uri(new Uri("http://example.com"), licenseExpression);
            return uri.AbsolutePath.Substring(1);
        }

        private LicenseExpressionDecodingService _target;

        public LicenseExpressionDecodingServiceFacts()
        {
            _target = new LicenseExpressionDecodingService();
        }
    }
}
