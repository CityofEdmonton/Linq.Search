using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit;

namespace CityofEdmonton.Linq.Search.Query
{
    public class NavigationPropertyTest : QueryTestBase
    {
        public NavigationPropertyTest()
        {
            SearchConfiguration.ConfigureSearch(options =>
            {
                var employeeConfig = options.Entity<Employee>();
                employeeConfig.Map(e => e.Manager.City, "ManagerCity");
                employeeConfig.Map(e => e.Manager == null ? "" : e.Manager.FirstName + " " + e.Manager.LastName, "Manager");

                var customerConfig = options.Entity<Customer>();
                customerConfig.Map(c => c.Orders.Select(y => y.ShipName), "ShipName");
                customerConfig.Map(c => c.Orders.Select(y => y.Freight), "Freight");
            });
        }

        [Fact]
        public void CanSearchNavigationPropertyExact()
        {
            var query = Employees.Where(x => x.Manager != null)
                .Search("ManagerCity: Tacoma");
            query.Should().NotBeNullOrEmpty();
            query.Count().Should().Be(5);
        }

        [Fact]
        public void CanSearchNavigationPropertyPartial()
        {
            var query = Employees.Where(x => x.Manager != null)
                .Search("ManagerCity: com"); // "com" part of Tacoma
            query.Should().NotBeNullOrEmpty();
            query.Count().Should().Be(5);
        }

        [Fact]
        public void CanSearchNavigationProjection()
        {
            var query = Employees.Search("Manager: Steven Buchanan");
            query.Should().NotBeNullOrEmpty();
            query.Count().Should().Be(3);
        }

        [Fact]
        public void CanSearchNavigationProjectionCaseInsensitive()
        {
            var query = Employees.Search("manager: steven buchanan");
            query.Should().NotBeNullOrEmpty();
            query.Count().Should().Be(3);
        }

        [Fact]
        public void CanSearchNavigationGreaterThan()
        {
            var query = Customers.Search("Freight > 38.24");

            query.Should().NotBeNullOrEmpty();
            query.Count().Should().Be(76);
        }

        [Fact]
        public void CanSearchNavigationGreaterThanOrEquals()
        {
            var query = Customers.Search("Freight >= 38.24");

            query.Should().NotBeNullOrEmpty();
            query.Count().Should().Be(77);
        }


        [Fact]
        public void CanSearchNavigationLessThan()
        {
            var query = Customers.Search("Freight < 2.4");

            query.Should().NotBeNullOrEmpty();
            query.Count().Should().Be(41);
        }

        [Fact]
        public void CanSearchNavigationLessThanOrEquals()
        {
            var query = Customers.Search("Freight <= 2.4");

            query.Should().NotBeNullOrEmpty();
            query.Count().Should().Be(42);
        }
    }
}
