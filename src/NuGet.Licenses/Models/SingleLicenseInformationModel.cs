using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NuGet.Licenses.Models
{
    public class SingleLicenseInformationModel
    {
        public SingleLicenseInformationModel(string title, string text)
        {
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        public string Title { get; }
        public string Text { get; }
    }
}