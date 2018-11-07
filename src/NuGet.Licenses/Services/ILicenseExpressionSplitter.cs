// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using NuGet.Licenses.Models;
using NuGet.Packaging.Licenses;

namespace NuGet.Licenses.Services
{
    /// <summary>
    /// Interface for a helper class that allows given the license expression string convert it to a 
    /// series of "runs" that allow to identify elements (license ids, exception ids, operators, etc.)
    /// of license expression inside that string individually.
    /// </summary>
    /// <remarks>
    /// The goal is to be able to "pretty print" the license expression on a web page: e.g given the
    /// (MIT OR ISC OR GPL WITH Classpath-exception)
    /// license expression we should be able to display it while linking individual elements to their
    /// respective license or exception URLs (so 'MIT' when displayed on a web page is a link to
    /// /MIT page, while 'OR', whitespace and parentheses are not).
    /// So we split the string into the series of "runs" each representing some element of the expression.
    /// 
    /// The complacated case is:
    /// 
    /// (((MIT OR   (ISC))))
    /// 
    /// i.e. we cannot just restore the whole sequence from the tree alone since there is some extra
    /// parentheses that have no representation in the expression tree.
    /// </remarks>
    public interface ILicenseExpressionSplitter
    {
        /// <summary>
        /// Given the root of the license expression tree restores the sequence of "runs" for a license
        /// expression represented by that tree. Only meaningful "runs" are returned by this method.
        /// </summary>
        /// <param name="licenseExpressionRoot">Root of the license expresion tree</param>
        /// <returns>The list of the runs restored from the tree.</returns>
        /// <remarks>
        /// This method only returns "runs" of types <see cref="CompositeLicenseExpressionRunType.LicenseIdentifier"/>,
        /// <see cref="CompositeLicenseExpressionRunType.ExceptionIdentifier"/>
        /// and <see cref="CompositeLicenseExpressionRunType.Operator"/>.
        /// 
        /// It cannot restore extra whitespace and parentheses that might have been present in the original expression.
        /// </remarks>
        List<CompositeLicenseExpressionRun> GetLicenseExpressionRuns(LicenseOperator licenseExpressionRoot);

        /// <summary>
        /// "Projects" the list of the runs provided by <see cref="GetLicenseExpressionRuns(LicenseOperator)"/> methods
        /// onto the license expression string discovering any extra "runs" of
        /// type <see cref="CompositeLicenseExpressionRunType.Other"/> it might have.
        /// </summary>
        /// <param name="licenseExpression">License expression string to get additional information from.</param>
        /// <param name="runs">List of the runs returned by <see cref="GetLicenseExpressionRuns(LicenseOperator)"/></param>
        /// <returns>The complete list of "runs" making up the license expression including any extra data it might have.</returns>
        List<CompositeLicenseExpressionRun> SplitFullExpression(string licenseExpression, IReadOnlyCollection<CompositeLicenseExpressionRun> runs);
    }
}