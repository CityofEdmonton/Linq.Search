using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace CityofEdmonton.Linq.Search.Query
{
    public class SimpleQueryTest : QueryTestBase
    {
        public SimpleQueryTest()
        {
            // Just use defaults for search
            SearchConfiguration.ConfigureSearch();
        }

        [Theory]
        [InlineData("ALFKI", 1)]
        [InlineData("Ana Trujillo Emparedados y helados", 1)]
        [InlineData("México", 5)]
        [InlineData("Sweden", 2)]
        public virtual void SimpleSearch(string searchString, int searchCount)
        {
            var customerQuery = Customers.Search(searchString);
            customerQuery.Should().NotBeNull();
            customerQuery.Should().HaveCount(searchCount);
        }
    }
}
