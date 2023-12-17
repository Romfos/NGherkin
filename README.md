# Overview

Modern Gherkin framework for .NET ecosystem for different types of tests

[![.github/workflows/build.yml](https://github.com/Romfos/NGherkin/actions/workflows/build.yml/badge.svg)](https://github.com/Romfos/NGherkin/actions/workflows/build.yml)

[![NGherkin](https://img.shields.io/nuget/v/NGherkin?label=NGherkin)](https://www.nuget.org/packages/NGherkin)\
[![NGherkin.TestAdapter](https://img.shields.io/nuget/v/NGherkin.TestAdapter?label=NGherkin.TestAdapter)](https://www.nuget.org/packages/NGherkin.TestAdapter)

# Philosophy
- Do not reinvent the wheel and use familiar for every .NET Developer conceptions like:
  - ASP.NET Core like Startup,
  - Microsoft.Extensions.DependencyInjection is a core
  - System.Text.Json\
  e.t.c
- Good integrated with modern .NET ecossystem.
- Editor plugins for editors should be optional. (maybe only syntax hightliting for gherkin, if needed)
- Minimalistic. Not a monster-framework with tons of custom conceptions.
- Support moderm .NET features like nullable referene types e.t.c
- No dependecies on other test runners\test frameworks

### What is NOT a goal
- Support deprecated framework versions
- Supprot old visual studio versions

# Requirements
- NET 6+ (recommended) or .NET Framework 4.6.2+
- Visual Studio 2022 or Visual Studio Code or any other editor with .NET support

Optional: gherking syntax plugin for your code editor:
1) Specflow for Visual Studio 2022,
2) Cucumber for Visual Studio Code or any other plugin

# How to use
1) Create new class library for .NET 6+ or .NET Framework 4.6.2+
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
    }

    [Then("then1")]
    public void Then1()
    {
    }
}
```
