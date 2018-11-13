﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using NuGet.Licenses.Models;
using NuGet.Packaging.Licenses;

namespace NuGet.Licenses.Services
{
    /// <summary>
    /// Interface for a helper class that allows given the license expression string convert it to a 
    /// series of "segments" that allow to identify elements (license ids, exception ids, operators, etc.)
    /// of license expression inside that string individually.
    /// </summary>
    /// <remarks>
    /// The goal is to be able to "pretty print" the license expression on a web page: e.g given the
    /// (MIT OR ISC OR GPL WITH Classpath-exception)
    /// license expression we should be able to display it while linking individual elements to their
    /// respective license or exception URLs (so 'MIT' when displayed on a web page is a link to
    /// /MIT page, while 'OR', whitespace and parentheses are not).
    /// So we split the string into the series of "segments" each representing some element of the expression.
    /// 
    /// The complicated case is:
    /// 
    /// (((MIT OR   (ISC))))
    /// 
    /// i.e. we cannot just restore the whole sequence from the tree alone since there is some extra
    /// parentheses that have no representation in the expression tree.
    /// </remarks>
    public interface ILicenseExpressionSplitter
    {
        /// <summary>
        /// Given the root of the license expression tree restores the sequence of "segments" for a license
        /// expression represented by that tree. Only meaningful (license or exceptions ids and operators) 
        /// "segments" are returned by this method.
        /// </summary>
        /// <param name="licenseExpressionRoot">Root of the license expresion tree</param>
        /// <returns>The list of the segments restored from the tree.</returns>
        /// <remarks>
        /// This method only returns "segments" of types <see cref="CompositeLicenseExpressionSegmentType.LicenseIdentifier"/>,
        /// <see cref="CompositeLicenseExpressionSegmentType.ExceptionIdentifier"/>
        /// and <see cref="CompositeLicenseExpressionSegmentType.Operator"/>.
        /// 
        /// It cannot restore extra whitespace and parentheses that might have been present in the original expression.
        /// </remarks>
        List<CompositeLicenseExpressionSegment> GetLicenseExpressionSegments(LicenseOperator licenseExpressionRoot);

        /// <summary>
        /// "Projects" the list of the segments provided by
        /// <see cref="GetLicenseExpressionSegments(LicenseOperator)"/> method
        /// onto the license expression string discovering any extra "segments" of
        /// type <see cref="CompositeLicenseExpressionSegmentType.Other"/> it might have.
        /// </summary>
        /// <param name="licenseExpression">License expression string to get additional information from.</param>
        /// <param name="segments">List of the segments returned by <see cref="GetLicenseExpressionSegments(LicenseOperator)"/></param>
        /// <returns>The complete list of "segments" making up the license expression including any extra data it might have.</returns>
        List<CompositeLicenseExpressionSegment> SplitFullExpression(string licenseExpression, IReadOnlyCollection<CompositeLicenseExpressionSegment> segments);
    }
}