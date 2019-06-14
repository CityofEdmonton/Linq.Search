# Linq.Search

A Super simple filtering library for IQueryables

[![Build Status](https://cityofedmonton.visualstudio.com/CoE%20Utilities/_apis/build/status/Linq.Search?branchName=master)](https://cityofedmonton.visualstudio.com/CoE%20Utilities/_build/latest?definitionId=26&branchName=master)

## What is Linq.Search?

Linq.Search is a super simple way to search collections that implement the IQueryable<T> interface.

## How to use it (examples, from [SimpleQueryTest.cs](https://github.com/CityofEdmonton/Linq.Search/blob/master/test/Linq.Search.Specification.Tests/Query/SimpleQueryTest.cs))

```csharp

using System.Linq;
using Linq.Search;

public class Customer
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public City City { get; set; }
}

public class City {
    public string Name { get; set; }
    public string Country { get; set; }
    public int Population { get; set;}
}

public class CustomerTest
{
    public static City Edmonton = new City { Name = "Edmonton", Country = "Canada", Population = 1000000 };
    public static City NewYork = new City { Name = "New York", Country = "United States", Population = 20000000 };

    public IQueryable<Customer> Customers = new List<Customer>()
    {
        new Customer() { FirstName = "Joe", LastName = "Smith", City = Edmonton },
        new Customer() { FirstName = "Jane", LastName = "Smith", City = NewYork },
        new Customer() { FirstName = "John", LastName = "Doe", City = NewYork }
    }.AsQueryable();

    public void SearchCustomers()
    {
        // No Configuration :
        Customers.Search("LastName: Smith"); // returns Joe and Jane Smith

        Customers.Search("Joe").Single(); // returns the single record of Joe Smith


        // Configuring searchable fields
        SearchConfiguration.ConfigureSearch(options =>
        {
            options.Entity<Customer>()
                .AddDefaultSearchField(c => c.FirstName)
                .AddDefaultSearchField(c => c.LastName)
                .AddDefaultSearchField(c => c.City.Name)
                .Map(c => c.FirstName + " " + c.LastName, alias: "name")
                .Map(c => c.City.Name, alias: "city")
                .Map(c => c.City.Country, alias: "country")
                .Map(c => c.City.Population, alias: "citypop")
                .Map(c => c.City.Name + " " + c.City.Country, alias: "cityinfo");
        });

        Customers.Search("Name: John OR Edmonton"); // returns John Doe and Joe Smith (in Edmonton)

        
        Customers.Search("name: Joe Smith OR name: John"); // return Joe Smith and John doe

        Customers.Search("city = new york"); // returns Jane and John Smith (who are in New York)

        Customers.Search("city: york"); // returns Jane and John Smith (who are in New York)

        Customers.Search("citypop < 5000000"); // returns Joe Smith in Edmonton

    } 
}
```

