using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit;

namespace CityofEdmonton.Linq.Search.Query
{
    public class MappingTest : QueryTestBase
    {
        [Theory]
        [InlineData("CompanyName")]
        public void SearchingNotMappedPropertyThrows(string propertyName)
        {
            SearchConfiguration.ConfigureSearch(options =>
            {
                options.Entity<Customer>().Map(propertyName);
            });

            var searchableProperties = new string[] { "CustomerID", "CompanyName", "ContactName", "ContactTitle", "Address", "City", "Region", "PostalCode", "Country", "Phone", "Fax" };
            foreach (var prop in searchableProperties)
            {


                if (string.Equals(prop, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    // this should work fine
                    Customers.Search(propertyName + ": whatever").Should().NotBeNull();
                }
                else
                {
                    // this should throw an exception beacuse it shouldn't know what to do with the property
                    Action searchAction = () => { Customers.Search(prop + ": whatever").ToArray(); };
                    searchAction.Should().Throw<SearchParseException>();
                }
            }
        }
    }
}
