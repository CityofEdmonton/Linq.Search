using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit;

namespace CityofEdmonton.Linq.Search.Query
{
    public class MappingTest : QueryTestBase
    {
        [Theory]
        [InlineData("CustomerID")]
        public void SearchingNotMappedPropertyThrows(string propertyName)
        {
            var searchableProperties = new string[] { "CustomerID", "CompanyName", "ContactName", "ContactTitle", "Address", "City", "Region", "PostalCode", "Country", "Phone", "Fax" };
            foreach (var prop in searchableProperties)
            {
                SearchConfiguration.ConfigureSearch(options =>
                {
                    options.Entity<Customer>().Map(propertyName);
                });

                if (string.Equals(prop, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    // this should work fine
                    Customers.Search()
                }
                else
                {

                }
            }
        }
    }
}
