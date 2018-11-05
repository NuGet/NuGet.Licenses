using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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