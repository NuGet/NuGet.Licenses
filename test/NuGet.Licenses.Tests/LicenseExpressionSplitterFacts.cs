using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet.Licenses.Models;
using NuGet.Licenses.Services;
using NuGet.Packaging.Licenses;
using Xunit;

namespace NuGet.Licenses.Tests
{
    public class TheGetLicenseExpressionRunsMethod : LicenseExpressionSplitterFactsBase
    {
        [Fact]
        public void ThrowsWhenLicenseExpressionRootIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => _target.GetLicenseExpressionRuns(null));
        }

        public static IEnumerable<object[]> LicenseExpressionsAndRuns => new object[][]
        {
            new object[] { "(MIT OR ISC)", new[] { License("MIT"), Or(), License("ISC") } },
            new object[] { "(((MIT  OR ISC)))", new[] { License("MIT"), Or(), License("ISC") } },
            new object[] { "(((MIT)) OR  ((ISC)))", new[] { License("MIT"), Or(), License("ISC") } },
            new object[] { "(MIT OR ISC  WITH Classpath-exception-2.0)", new[] { License("MIT"), Or(), License("ISC"), With(), Exception("Classpath-exception-2.0") } },
        };

        [Theory]
        [MemberData(nameof(LicenseExpressionsAndRuns))]
        public void ProducesProperSequenceOfRuns(string licenseExpression, ComplexLicenseExpressionRun[] expectedSequence)
        {
            var expressionTreeRoot = NuGetLicenseExpression.Parse(licenseExpression);

            var runs = _target.GetLicenseExpressionRuns((LicenseOperator)expressionTreeRoot);

            Assert.NotNull(runs);
            Assert.Equal(expectedSequence, runs, new ComplexLicenseExpressionRunEqualityComparer());
        }
    }

    public class TheSplitFullExpressionMethod : LicenseExpressionSplitterFactsBase
    {
        [Fact]
        public void ThrowsWhenLicenseExpressionIsNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => _target.SplitFullExpression(null, new ComplexLicenseExpressionRun[0]));
            Assert.Equal("licenseExpression", ex.ParamName);
        }

        [Fact]
        public void ThrowsWhenRunsIsNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => _target.SplitFullExpression("", null));
            Assert.Equal("runs", ex.ParamName);
        }

        public static IEnumerable<object[]> LicenseExpressionsAndRuns => new object[][]
        {
            new object[] {
                "(MIT OR ISC)",
                new[] { License("MIT"), Or(), License("ISC") },
                new[] { Other("("), License("MIT"), Other(" "), Or(), Other(" "), License("ISC"), Other(")") }
            },
            new object[] {
                "(((MIT  OR ISC)))",
                new[] { License("MIT"), Or(), License("ISC") },
                new[] { Other("((("), License("MIT"), Other("  "), Or(), Other(" "), License("ISC"), Other(")))") }
            },
            new object[] {
                "(((MIT)) OR  ((ISC)))",
                new[] { License("MIT"), Or(), License("ISC") },
                new[] { Other("((("), License("MIT"), Other(")) "), Or(), Other("  (("), License("ISC"), Other(")))") }
            },
            new object[] {
                "(MIT OR ISC  WITH Classpath-exception-2.0)",
                new[] { License("MIT"), Or(), License("ISC"), With(), Exception("Classpath-exception-2.0") },
                new[] { Other("("), License("MIT"), Other(" "), Or(), Other(" "), License("ISC"), Other("  "), With(), Other(" "), Exception("Classpath-exception-2.0"), Other(")") }
            },
        };

        [Theory]
        [MemberData(nameof(LicenseExpressionsAndRuns))]
        public void AddsParenthesesAndWhitespace(string licenseExpression, ComplexLicenseExpressionRun[] runs, ComplexLicenseExpressionRun[] expectedRuns)
        {
            var result = _target.SplitFullExpression(licenseExpression, runs);

            Assert.Equal(expectedRuns, result, new ComplexLicenseExpressionRunEqualityComparer());
        }
    }


    public class LicenseExpressionSplitterFactsBase
    {
        protected LicenseExpressionSplitter _target;

        public LicenseExpressionSplitterFactsBase()
        {
            _target = new LicenseExpressionSplitter();
        }

        protected static ComplexLicenseExpressionRun License(string licenseId)
            => new ComplexLicenseExpressionRun(licenseId, ComplexLicenseExpressionRunType.LicenseIdentifier);

        protected static ComplexLicenseExpressionRun Operator(string operatorName)
            => new ComplexLicenseExpressionRun(operatorName, ComplexLicenseExpressionRunType.Operator);

        protected static ComplexLicenseExpressionRun Exception(string exceptionId)
            => new ComplexLicenseExpressionRun(exceptionId, ComplexLicenseExpressionRunType.ExceptionIdentifier);

        protected static ComplexLicenseExpressionRun Or() => Operator("OR");
        protected static ComplexLicenseExpressionRun And() => Operator("AND");
        protected static ComplexLicenseExpressionRun With() => Operator("WITH");

        protected static ComplexLicenseExpressionRun Other(string value)
            => new ComplexLicenseExpressionRun(value, ComplexLicenseExpressionRunType.Other);
    }

    internal class ComplexLicenseExpressionRunEqualityComparer : IEqualityComparer<ComplexLicenseExpressionRun>
    {
        public bool Equals(ComplexLicenseExpressionRun x, ComplexLicenseExpressionRun y)
        {
            if (x == null || y == null)
            {
                return x == y;
            }

            return x.Type == y.Type && x.Value == y.Value;
        }

        public int GetHashCode(ComplexLicenseExpressionRun obj)
        {
            return obj.Type.GetHashCode() ^ obj.Value.GetHashCode();
        }
    }
}
