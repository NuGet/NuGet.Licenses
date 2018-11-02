using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NuGet.Licenses.Models
{
    public class UnknownLicenseModel
    {
        public UnknownLicenseModel(string licenseName)
        {
            LicenseName = licenseName ?? throw new ArgumentNullException(nameof(licenseName));
        }

        public string LicenseName { get; }
    }
}