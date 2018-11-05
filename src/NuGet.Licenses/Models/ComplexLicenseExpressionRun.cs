using System;

namespace NuGet.Licenses.Models
{
    /// <summary>
    /// Represents a portion of a complex license expression allowing to specify its type.
    /// </summary>
    public class ComplexLicenseExpressionRun
    {
        public ComplexLicenseExpressionRun(string textValue, ComplexLicenseExpressionRunType type)
        {
            Value = textValue ?? throw new ArgumentNullException(nameof(textValue));
            Type = type;
        }

        /// <summary>
        /// Text value of the license expression portion
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Type of the license expression option
        /// </summary>
        public ComplexLicenseExpressionRunType Type { get; }
    }
}