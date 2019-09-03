# Linq.Search

A Super simple filtering library for IQueryables

[![Build Status](https://cityofedmonton.visualstudio.com/CoE%20Utilities/_apis/build/status/Linq.Search?branchName=master)](https://cityofedmonton.visualstudio.com/CoE%20Utilities/_build/latest?definitionId=26&branchName=master)

## What is Linq.Search?

Linq.Search is a super simple way to search collections that implement the IQueryable<T> interface. Works with Linq to Objects, Entity Framework, EntityFrameworkCore, and any other LINQ provider.

## Usage (examples using Northwind database)

### Searching multiple fields (basic search)

```csharp
// Returns query for all public string fields with 'maria' anywhere in the string
IQueryable<Customer> query = customers.Search("maria");

// Returns query where 'hungry' and 'yoshi' appear in any of the fields.
// The search terms can be in different properties.
query = customers.Search("hungry and yoshi");

// We can use AND or OR in our searches.
query = customers.Search("maria or yoshi");
```

### Searching multiple fields (Configuration)

```csharp
// Instead of searching all public string properties,
// we can configure which ones to search,
// which can include related entities.
SearchConfiguration.ConfigureSearch(options => 
{
    options.Entity<Order>()
        .AddDefaultSearchField(o => o.ShipCity)
        .AddDefaultSearchField(o => o.Customer.ContactName)
        .AddDefaultSearchField(o => o.OrderDetails.Select(od => od.Product.ProductName));
});

// Find orders with "tofu" in the city, contactName or productName
query = orders.Search("tofu");
```

### Searching specific properties

```csharp
// By default we can search any properties directly on the entity being searched
IQueryable<Customer> query = customers.Search("ContactName: Maria");

// We can also search for exact matches
query = customers.Search("ContactName = Maria Anders");

// We can mix searching the default fields with named properties.
query = customers.Search("ContactName: Maria OR yoshi");

// We can configure the search terms and also reference related entities.
SearchConfiguration.ConfigureSearch(options => 
{
    options.Entity<Order>()
        .Map(o => o.Customer.ContactName, "customer")
        .Map(o => o.OrderDetails.Select(od => od.Product.ProductName), "product");
});

// Search for orders by carlos that contain tofu
query = orders.Search("customer: carlos and product: tofu");
```

### Searching using Comparison Operators

```csharp
SearchConfiguration.ConfigureSearch(options => 
{
    options.Entity<Product>()
        .Map(p => p.ProductName, "product")
        .Map(p => p.UnitPrice, "productPrice")
        .Map(p => p.UnitsInStock, "productStock");
});

// Any of (>, >=, < or <=) can be used for comparison on numeric fields
IQueryable<Order> query = products.Search("productPrice > 10 and productStock <= 5");

```

### More examples (.NET Fiddle):
| Name                  | Link                            |
| --------------------- | ------------------------------- |
| Basic Search          | https://dotnetfiddle.net/Ox2owv |
| Navigation Properties | https://dotnetfiddle.net/igWruA |
| Comparison Operators  | https://dotnetfiddle.net/iaIgqI |

More examples can be found in the [tests project in this repository](https://github.com/CityofEdmonton/Linq.Search/tree/master/test/Linq.Search.Specification.Tests/Query).


# Installation

Linq.Search is available as a [nuget package](https://www.nuget.org/packages/Linq.Search/) for .NET 4.5+ and .NET Core 2.0+. 

# Building from source

Visual Studio 2017 or Visual Studio 2019 is recommended for building and running the tests.  Just open the .sln file at the root of this repository and build the project. 
