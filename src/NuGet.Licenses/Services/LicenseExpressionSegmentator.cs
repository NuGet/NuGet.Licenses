﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using NuGet.Licenses.Models;
using NuGet.Packaging.Licenses;

namespace NuGet.Licenses.Services
{
    public class LicenseExpressionSegmentator : ILicenseExpressionSegmentator
    {
        /// <summary>
        /// Does an in-order traversal of a license expression tree restoring the sequence of tokens
        /// used in the expression (omitting all parentheses and whitespace)
        /// </summary>
        /// <param name="licenseExpressionRoot">Root of the license expression tree</param>
        /// <returns>The list of license expression token in the order they appeared in the original expression.</returns>
        public List<CompositeLicenseExpressionSegment> GetLicenseExpressionSegments(LicenseOperator licenseExpressionRoot)
        {
            if (licenseExpressionRoot == null)
            {
                throw new ArgumentNullException(nameof(licenseExpressionRoot));
            }

            var segmentList = new List<CompositeLicenseExpressionSegment>();
            InOrderTraversal(licenseExpressionRoot, segmentList);
            return segmentList;
        }

        /// <summary>
        /// Given the original license expression and list of segments produced by <see cref="GetLicenseExpressionSegments"/>
        /// produces full split of the expression into list of segments that include all the characters of the original
        /// license expression (including the parentheses and whitespace).
        /// </summary>
        /// <param name="licenseExpression">Original license expression.</param>
        /// <param name="segments">List of segments produced by <see cref="GetLicenseExpressionSegments"/></param>
        /// <returns>List of segments including the characters that are lost during expression parsing</returns>
        public List<CompositeLicenseExpressionSegment> SplitFullExpression(string licenseExpression, IReadOnlyCollection<CompositeLicenseExpressionSegment> segments)
        {
            if (licenseExpression == null)
            {
                throw new ArgumentNullException(nameof(licenseExpression));
            }

            if (segments == null)
            {
                throw new ArgumentNullException(nameof(segments));
            }

            var fullSegmentList = new List<CompositeLicenseExpressionSegment>();
            var startIndex = 0;
            foreach (var segment in segments)
            {
                var currentSegmentStartIndex = licenseExpression.IndexOf(segment.Value, startIndex);
                if (currentSegmentStartIndex < 0)
                {
                    throw new InvalidOperationException($"Unable to find '{segment.Value}' portion of the license expression starting from {startIndex} in '{licenseExpression}'");
                }
                if (currentSegmentStartIndex > startIndex)
                {
                    fullSegmentList.Add(
                        new CompositeLicenseExpressionSegment(licenseExpression.Substring(startIndex, currentSegmentStartIndex - startIndex),
                        CompositeLicenseExpressionSegmentType.Other));
                }
                fullSegmentList.Add(segment);
                startIndex = currentSegmentStartIndex + segment.Value.Length;
            }

            if (startIndex < licenseExpression.Length)
            {
                fullSegmentList.Add(
                    new CompositeLicenseExpressionSegment(licenseExpression.Substring(startIndex),
                    CompositeLicenseExpressionSegmentType.Other));
            }

            return fullSegmentList;
        }

        private static void InOrderTraversal(NuGetLicenseExpression root, List<CompositeLicenseExpressionSegment> segmentList)
        {
            switch (root.Type)
            {
                case LicenseExpressionType.License:
                    {
                        var licenseNode = (NuGetLicense)root;
                        segmentList.Add(new CompositeLicenseExpressionSegment(licenseNode.Identifier, CompositeLicenseExpressionSegmentType.LicenseIdentifier));
                        if (licenseNode.Plus)
                        {
                            segmentList.Add(new CompositeLicenseExpressionSegment("+", CompositeLicenseExpressionSegmentType.Operator));
                        }
                    }
                    break;

                case LicenseExpressionType.Operator:
                    {
                        var operatorNode = (LicenseOperator)root;
                        if (operatorNode.OperatorType == LicenseOperatorType.LogicalOperator)
                        {
                            var logicalOperator = (LogicalOperator)operatorNode;
                            InOrderTraversal(logicalOperator.Left, segmentList);
                            segmentList.Add(new CompositeLicenseExpressionSegment(GetLogicalOperatorString(logicalOperator), CompositeLicenseExpressionSegmentType.Operator));
                            InOrderTraversal(logicalOperator.Right, segmentList);

                        }
                        else if (operatorNode.OperatorType == LicenseOperatorType.WithOperator)
                        {
                            var withOperator = (WithOperator)operatorNode;
                            InOrderTraversal(withOperator.License, segmentList);
                            segmentList.Add(new CompositeLicenseExpressionSegment("WITH", CompositeLicenseExpressionSegmentType.Operator));
                            segmentList.Add(new CompositeLicenseExpressionSegment(withOperator.Exception.Identifier, CompositeLicenseExpressionSegmentType.ExceptionIdentifier));
                        }
                        else
                        {
                            throw new InvalidOperationException($"Unknown operator type: {operatorNode.OperatorType}");
                        }
                    }
                    break;
            }
        }

        private static string GetLogicalOperatorString(LogicalOperator logicalOperator)
        {
            switch (logicalOperator.LogicalOperatorType)
            {
                case LogicalOperatorType.And:
                    return "AND";

                case LogicalOperatorType.Or:
                    return "OR";

                default:
                    throw new InvalidOperationException($"Unsupported logical operator type: {logicalOperator.LogicalOperatorType}");
            }
        }
    }
}