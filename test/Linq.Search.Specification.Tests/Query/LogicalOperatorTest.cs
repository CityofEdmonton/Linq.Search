using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Xunit;

namespace CityofEdmonton.Linq.Search.Query
{
    public class LogicalOperatorTest : QueryTestBase
    {
        public LogicalOperatorTest()
        {
            // Just use defaults for search
            SearchConfiguration.ConfigureSearch();
        }

        [Theory]
        [InlineData("Country: Germany", "ContactName: Frank")]
        public void AndContainsTest(string left, string right)
        {
            // We should get multiple customers with our first query
            var query = Customers.Search(left);
            query.Should().NotBeEmpty();
            query.Count().Should().BeGreaterOrEqualTo(2);

            // Applying the second query after our first should give us a smaller set
            var withRight = query.Search(right);
            withRight.Should().NotBeEmpty();
            query.Count().Should().BeGreaterThan(withRight.Count());

            // The interseaction (AND) on the original querable should
            // be the same as applying the queries one after the other
            var andQuery = Customers.Search(left + " and " + right);
            andQuery.Should().NotBeNull();
            andQuery.Count().Should().Be(withRight.Count());
        }

        [Theory]
        [InlineData("Canada", "Country: France")]
        [InlineData("Country: Germany", "ContactName: Frank")]
        public void OrContainsTest(string left, string right)
        {
            // We should get customers with our first query
            var query = Customers.Search(left).ToArray();

            // We should get customers with our second query
            var query2 = Customers.Search(right).ToArray();

            var recordsInBothLeftAndRight = query
                .Where(x => query2.Any(y => y.CustomerID == x.CustomerID))
                .ToArray();

            var recordsInLeftButNotInRight = query
                .Where(x => !query2.Any(y => y.CustomerID == x.CustomerID))
                .ToArray();

            var recordsInRightButNotInLeft = query2
                .Where(x => !query.Any(y => y.CustomerID == x.CustomerID))
                .ToArray();

            // finally, the OR search query should give us the same number of records
            // as the # in both + # in left + # in right
            // (think of a Venn diagram)
            Customers.Search(left + " or " + right)
                .Count()
                .Should().Be(
                    recordsInBothLeftAndRight.Count() +
                    recordsInLeftButNotInRight.Count() +
                    recordsInRightButNotInLeft.Count()
                );
        }
    }
}
