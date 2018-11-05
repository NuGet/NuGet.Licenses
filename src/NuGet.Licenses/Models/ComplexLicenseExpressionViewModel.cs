using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NuGet.Licenses.Models
{
    public class ComplexLicenseExpressionViewModel
    {
        public ComplexLicenseExpressionViewModel(IReadOnlyCollection<ComplexLicenseExpressionRun> runs)
        {
            Runs = runs ?? throw new ArgumentNullException(nameof(runs));
        }

        public IReadOnlyCollection<ComplexLicenseExpressionRun> Runs { get; }
    }
}