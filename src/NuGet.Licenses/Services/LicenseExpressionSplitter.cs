using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NuGet.Licenses.Models;
using NuGet.Packaging.Licenses;

namespace NuGet.Licenses.Services
{
    public class LicenseExpressionSplitter
    {
        /// <summary>
        /// Does an in-order traversal of a license expression tree restoring the sequence of tokens
        /// used in the expression (omitting all parentheses an whitespace)
        /// </summary>
        /// <param name="licenseExpressionRoot">Root of the license expression tree</param>
        /// <returns>The list of license expression token in the order they appeared in the original expression.</returns>
        public List<ComplexLicenseExpressionRun> GetLicenseExpressionRuns(LicenseOperator licenseExpressionRoot)
        {
            var runList = new List<ComplexLicenseExpressionRun>();
            InOrderTraversal(licenseExpressionRoot, runList);
            return runList;
        }

        private static void InOrderTraversal(NuGetLicenseExpression root, List<ComplexLicenseExpressionRun> runList)
        {
            switch (root.Type)
            {
                case LicenseExpressionType.License:
                    {
                        var licenseNode = (NuGetLicense)root;
                        runList.Add(new ComplexLicenseExpressionRun(licenseNode.Identifier, ComplexLicenseExpressionRunType.LicenseIdentifier));
                        if (licenseNode.Plus)
                        {
                            runList.Add(new ComplexLicenseExpressionRun("+", ComplexLicenseExpressionRunType.Operator));
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
                            runList.Add(new ComplexLicenseExpressionRun(GetLogicalOperatorString(logicalOperator), ComplexLicenseExpressionRunType.Operator));
                            InOrderTraversal(logicalOperator.Right, runList);

                        }
                        else if (operatorNode.OperatorType == LicenseOperatorType.WithOperator)
                        {
                            var withOperator = (WithOperator)operatorNode;
                            InOrderTraversal(withOperator.License, runList);
                            runList.Add(new ComplexLicenseExpressionRun("WITH", ComplexLicenseExpressionRunType.Operator));
                            runList.Add(new ComplexLicenseExpressionRun(withOperator.Exception.Identifier, ComplexLicenseExpressionRunType.ExceptionIdentifier));
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