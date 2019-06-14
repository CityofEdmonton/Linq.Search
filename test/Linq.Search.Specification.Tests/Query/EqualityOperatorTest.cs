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
    public class EqualityOperatorTest : QueryTestBase
    {
        public EqualityOperatorTest()
        {
            // just use defaults
            SearchConfiguration.ConfigureSearch();
        }

        [Fact]
        public void CanSearchEquals()
        {
            var query = Products.Search("UnitPrice = 15.5");
            query.Should().NotBeNullOrEmpty();
            query.Count().Should().Be(1);
        }

        [Fact]
        public void CanSearchGreaterThan()
        {
            var query = Products.Search("UnitPrice > 30");
            query.Should().NotBeNullOrEmpty();
            query.Count().Should().Be(24);
        }

        [Fact]
        public void CanSearchGreaterThanOrEquals()
        {
            var query = Products.Search("UnitPrice >= 30");
            query.Should().NotBeNullOrEmpty();
            query.Count().Should().Be(25);
        }

        [Fact]
        public void CanSearchLessThan()
        {
            var query = Products.Search("UnitPrice < 30");
            query.Should().NotBeNullOrEmpty();
            query.Count().Should().Be(52);
        }

        [Fact]
        public void CanSearchLessThanOrEquals()
        {
            var query = Products.Search("UnitPrice <= 30");
            query.Should().NotBeNullOrEmpty();
            query.Count().Should().Be(53);
        }

    }
}
