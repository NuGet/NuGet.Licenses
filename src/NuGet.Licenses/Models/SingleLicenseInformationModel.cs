// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace NuGet.Licenses.Models
{
    public class SingleLicenseInformationModel
    {
        public SingleLicenseInformationModel(string title, string text, bool isException)
        {
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            IsException = isException;
        }

        public string Title { get; }
        public string Text { get; }
        public bool IsException { get; }
    }
}