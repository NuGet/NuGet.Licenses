// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace NuGet.Licenses.Models
{
    public class InvalidRequestModel
    {
        public InvalidRequestModel(string errorMessage = null)
        {
            Message = errorMessage; // OK to be null
        }

        public string Message { get; }
    }
}