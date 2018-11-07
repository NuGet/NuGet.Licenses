// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using NuGet.Licenses.Models;
using NuGet.Packaging.Licenses;

namespace NuGet.Licenses.Services
{
    public class LicenseExpressionSplitter : ILicenseExpressionSplitter
    {
        /// <summary>
        /// Does an in-order traversal of a license expression tree restoring the sequence of tokens
        /// used in the expression (omitting all parentheses an whitespace)
        /// </summary>
        /// <param name="licenseExpressionRoot">Root of the license expression tree</param>
        /// <returns>The list of license expression token in the order they appeared in the original expression.</returns>
        public List<CompositeLicenseExpressionRun> GetLicenseExpressionRuns(LicenseOperator licenseExpressionRoot)
        {
            if (licenseExpressionRoot == null)
            {
                throw new ArgumentNullException(nameof(licenseExpressionRoot));
            }

            var runList = new List<CompositeLicenseExpressionRun>();
            InOrderTraversal(licenseExpressionRoot, runList);
            return runList;
        }

        /// <summary>
        /// Given the original license expression and list of runs produced by <see cref="GetLicenseExpressionRuns"/>
        /// produces full split of the expression into list of runs that include all the characters of the original
        /// license expression (including the parentheses and whitespace).
        /// </summary>
        /// <param name="licenseExpression">Original license expression.</param>
        /// <param name="runs">List of runs produced by <see cref="GetLicenseExpressionRuns"/></param>
        /// <returns>List of runs including the characters that are lost during expression parsing</returns>
        public List<CompositeLicenseExpressionRun> SplitFullExpression(string licenseExpression, IReadOnlyCollection<CompositeLicenseExpressionRun> runs)
        {
            if (licenseExpression == null)
            {
                throw new ArgumentNullException(nameof(licenseExpression));
            }

            if (runs == null)
            {
                throw new ArgumentNullException(nameof(runs));
            }

            var fullRunList = new List<CompositeLicenseExpressionRun>();
            var startIndex = 0;
            foreach (var run in runs)
            {
                var currentRunStartIndex = licenseExpression.IndexOf(run.Value, startIndex);
                if (currentRunStartIndex < 0)
                {
                    throw new InvalidOperationException($"Unable to find '{run.Value}' portion of the license expression starting from {startIndex} in '{licenseExpression}'");
                }
                if (currentRunStartIndex > startIndex)
                {
                    fullRunList.Add(
                        new CompositeLicenseExpressionRun(licenseExpression.Substring(startIndex, currentRunStartIndex - startIndex),
                        CompositeLicenseExpressionRunType.Other));
                }
                fullRunList.Add(run);
                startIndex = currentRunStartIndex + run.Value.Length;
            }

            if (startIndex < licenseExpression.Length)
            {
                fullRunList.Add(
                    new CompositeLicenseExpressionRun(licenseExpression.Substring(startIndex),
                    CompositeLicenseExpressionRunType.Other));
            }

            return fullRunList;
        }

        private static void InOrderTraversal(NuGetLicenseExpression root, List<CompositeLicenseExpressionRun> runList)
        {
            switch (root.Type)
            {
                case LicenseExpressionType.License:
                    {
                        var licenseNode = (NuGetLicense)root;
                        runList.Add(new CompositeLicenseExpressionRun(licenseNode.Identifier, CompositeLicenseExpressionRunType.LicenseIdentifier));
                        if (licenseNode.Plus)
                        {
                            runList.Add(new CompositeLicenseExpressionRun("+", CompositeLicenseExpressionRunType.Operator));
                        }
                    }
                    break;

                case LicenseExpressionType.Operator:
                    {
                        var operatorNode = (LicenseOperator)root;
                        if (operatorNode.OperatorType == LicenseOperatorType.LogicalOperator)
                        {
                            var logicalOperator = (LogicalOperator)operatorNode;
                            InOrderTraversal(logicalOperator.Left, runList);
                            runList.Add(new CompositeLicenseExpressionRun(GetLogicalOperatorString(logicalOperator), CompositeLicenseExpressionRunType.Operator));
                            InOrderTraversal(logicalOperator.Right, runList);

                        }
                        else if (operatorNode.OperatorType == LicenseOperatorType.WithOperator)
                        {
                            var withOperator = (WithOperator)operatorNode;
                            InOrderTraversal(withOperator.License, runList);
                            runList.Add(new CompositeLicenseExpressionRun("WITH", CompositeLicenseExpressionRunType.Operator));
                            runList.Add(new CompositeLicenseExpressionRun(withOperator.Exception.Identifier, CompositeLicenseExpressionRunType.ExceptionIdentifier));
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