# Overview

Modern Gherkin framework for .NET ecosystem for different types of tests

[![.github/workflows/build.yml](https://github.com/Romfos/NGherkin/actions/workflows/build.yml/badge.svg)](https://github.com/Romfos/NGherkin/actions/workflows/build.yml)

[![NGherkin](https://img.shields.io/nuget/v/NGherkin?label=NGherkin)](https://www.nuget.org/packages/NGherkin)\
[![NGherkin.TestAdapter](https://img.shields.io/nuget/v/NGherkin.TestAdapter?label=NGherkin.TestAdapter)](https://www.nuget.org/packages/NGherkin.TestAdapter)

# Philosophy
- Should do 1 thing. Not an monster-framework with tons of custom conseptions.
- Should use familiar conceptions for every .NET Developer like Microsoft.Extensions.DependencyInjection, System.Text.Json, e.t.c
- Should be good and fast for modern .NET ecossystem. Depricated framework versions or legacy visual studio versions support is out of scope.
- Should support moderm .NET features like nullable referene types, e.t.c
- Editor plugins for editors should be optional. (maybe only syntax hightliting for gherkin, if needed)
- No code generation, no dependecies on other test runners and as the result - no compatibility problems with it
- Should be good choice for the next years.

# Requirements
- NET 6+ (recommended) or .NET Framework 4.6.2+ (only .NET sdk projects types)
- Visual Studio 2022 or Visual Studio Code
- Specflow plugin for Visual Studio (for gherking syntax highliting)

# Nuget packages links  
- https://www.nuget.org/packages/NGherkin
- https://www.nuget.org/packages/NGherkin.TestAdapter

# How to use
1) Create new class library with .NET sdk project type
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

    [When("when1")]
    public void When1()
    {
    }

    [Then("then1")]
    public void Then1()
    {
    }
}
```
