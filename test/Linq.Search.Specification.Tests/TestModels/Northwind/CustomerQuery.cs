// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.TestModels.Northwind
{
    public class CustomerQuery
    {
        public string CompanyName { get; set; }
        public int OrderCount { get; set; }
        public string SearchTerm { get; set; }
    }
}
