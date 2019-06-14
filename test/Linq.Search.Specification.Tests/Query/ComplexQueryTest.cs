using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
using Linq.Search;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit;

namespace CityofEdmonton.Linq.Search.Query
{
    public class ComplexQueryTest : QueryTestBase
    {
        public ComplexQueryTest()
        {
            SearchConfiguration.ConfigureSearch(options =>
            {
                options.Entity<Product>()
                    .Map(p => p.OrderDetails
                            .Select(od => new { od.Order.Customer.ContactTitle, od.Order.Customer.ContactName })
                            .Select(customerInfo => customerInfo.ContactTitle + " " + customerInfo.ContactName)
                            .LastOrDefault(),
                        "Contact");
            });
        }

        [Fact]
        public void CanSearchComplexNavigation()
        {
            Products.Search("Contact: owner maria").Count().Should().Be(2);
        }
    }
}
