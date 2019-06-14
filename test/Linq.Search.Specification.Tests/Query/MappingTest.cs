using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Linq.Search;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit;

namespace CityofEdmonton.Linq.Search.Query
{
    public class MappingTest : QueryTestBase
    {
        [Theory]
        [InlineData("CustomerID")]
        [InlineData("CompanyName")]
        [InlineData("ContactName")]
        [InlineData("ContactTitle")]
        [InlineData("Address")]
        [InlineData("City")]
        [InlineData("Region")]
        [InlineData("PostalCode")]
        [InlineData("Country")]
        [InlineData("Phone")]
        [InlineData("Fax")]
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

        [Fact]
        public void CanSearchByAlias()
        {
            SearchConfiguration.ConfigureSearch(options =>
            {
                options.Entity<Customer>().Map(c => c.City, "location", "place", "town");
            });

            Customers.Search("location: london").Count().Should().Be(6);
            Customers.Search("place: lond").Count().Should().Be(6);
            Customers.Search("town: ondon").Count().Should().Be(6);

        }
    }
}
