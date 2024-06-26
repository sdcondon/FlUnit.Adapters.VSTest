﻿![FlUnit Logo](src/FlUnitIcon.png)

# FlUnit.VS.TestAdapter

[![NuGet version (FlUnit.VS.TestAdapter)](https://img.shields.io/nuget/v/FlUnit.VS.TestAdapter.svg?style=flat-square)](https://www.nuget.org/packages/FlUnit.VS.TestAdapter/) 
[![NuGet downloads (FlUnit.Adapters.VSTest)](https://img.shields.io/nuget/dt/FlUnit.VS.TestAdapter.svg?style=flat-square)](https://www.nuget.org/packages/FlUnit.VS.TestAdapter/) 
[![Commits since latest release](https://img.shields.io/github/commits-since/sdcondon/FlUnit.Adapters.VSTest/latest?style=flat-square)](https://github.com/sdcondon/FlUnit.Adapters.VSTest/compare/2.0.0...main)

This package contains FlUnit's adapter for the VSTest platform - enabling VSTest to find and run FlUnit tests.
FlUnit is a test framework in which tests are written using [builders](https://en.wikipedia.org/wiki/Builder_pattern) that expose a [fluent interface](https://en.wikipedia.org/wiki/Fluent_interface), like this:

```csharp
public static Test SumOfEvenAndOdd => TestThat
  .GivenEachOf(() => new[] { 1, 3, 5 })
  .AndEachOf(() => new[] { 2, 4, 6 })
  .When((x, y) => x + y)
  .ThenReturns()
  .And((_, _, sum) => (sum % 2).Should().Be(1))
  .And((x, _, sum) => sum.Should().BeGreaterThan(x))
  .And((_, y, sum) => sum.Should().BeGreaterThan(y));
```

Full documentation can be found via the [FlUnit repository readme](https://github.com/sdcondon/FlUnit).
