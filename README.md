# Overview

Modern Gherkin framework for .NET ecosystem for different types of tests

[![.github/workflows/build.yml](https://github.com/Romfos/NGherkin/actions/workflows/build.yml/badge.svg)](https://github.com/Romfos/NGherkin/actions/workflows/build.yml)

[![NGherkin](https://img.shields.io/nuget/v/NGherkin?label=NGherkin)](https://www.nuget.org/packages/NGherkin)\
[![NGherkin.TestAdapter](https://img.shields.io/nuget/v/NGherkin.TestAdapter?label=NGherkin.TestAdapter)](https://www.nuget.org/packages/NGherkin.TestAdapter)

# Philosophy
- Not an monster-framework with tons of custom conseptions.
- Should be good and fast for integration with modern .NET ecossystem.
- No need to support depricated frameworks versions or old visual studio's
- Should support moderm .NET features like nullable referene types, e.t.c
- Do not reinvent the wheel and use familiar conceptions for every .NET Developer like startup, di, e.t.c
- Editor plugins for editors should be optional. (maybe only syntax hightliting for gherkin, if needed)
- No dependecies on other test runners\test frameworks
- Should be a good choice for the next years.

# Requirements
- NET 6+ (recommended) or .NET Framework 4.6.2+ (only .NET sdk projects types)
- Visual Studio 2022 or Visual Studio Code

Optional: gherking syntax plugin for your code editor:
1) Specflow for Visual Studio 2022,
2) Cucumber for Visual Studio Code or any other plugin

# Nuget packages links  
- https://www.nuget.org/packages/NGherkin
- https://www.nuget.org/packages/NGherkin.TestAdapter

# How to use
1) Create new class library for .NET 6+ (or SDK project for .NET Framework 4.6.2+)
2) Add following nuget packages:
- https://www.nuget.org/packages/NGherkin
- https://www.nuget.org/packages/NGherkin.TestAdapter
- https://www.nuget.org/packages/Microsoft.NET.Test.Sdk
3) Create startup class and register dependencies. Example:
  
```csharp
public sealed class Startup : StartupBase
{
    public override void Configure(IServiceCollection services)
    {
        services.AddGherkinFeatures();
        services.AddGherkinSteps();
    }
}

```

4) Add feature files
5) Add classes with steps. Example:

```csharp
[Steps]
internal sealed class StepClass
{
    [Given("given1")]
    public void Given()
    {
    }

    [When("this is when step with '(.*)' argument an '(.*)' argument")]
    public void When1(int arg1, string arg2, DataTable dataTable)
    {
        if (arg1 is not 1)
        {
            throw new ArgumentException(nameof(arg1));
        }

        if (arg2 is not "text")
        {
            throw new ArgumentException(nameof(arg2));
        }

        if (dataTable is null)
        {
            throw new ArgumentException(nameof(dataTable));
        }
    }

    [Then("then1")]
    public void Then1()
    {
    }
}
```
