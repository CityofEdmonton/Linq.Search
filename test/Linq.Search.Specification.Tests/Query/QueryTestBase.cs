﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit;

namespace CityofEdmonton.Linq.Search.Query
{
    // inspired by https://github.com/aspnet/EntityFrameworkCore/blob/master/test/EFCore.Specification.Tests/Query/QueryTestBase.cs
    public partial class QueryTestBase
    {
        static QueryTestBase()
        {
            _northwindData = new NorthwindData();
        }

        // Since these tests do not modify the collections, we can make the data static for efficiency
        private static readonly NorthwindData _northwindData;

        protected IQueryable<Customer> Customers => _northwindData.Set<Customer>();


    }
}
