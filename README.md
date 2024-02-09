# Overview

Modern Gherkin-based BDD framework for .NET ecosystem

[![.github/workflows/build.yml](https://github.com/Romfos/NGherkin/actions/workflows/build.yml/badge.svg)](https://github.com/Romfos/NGherkin/actions/workflows/build.yml)

[![NGherkin](https://img.shields.io/nuget/v/NGherkin?label=NGherkin)](https://www.nuget.org/packages/NGherkin)\
[![NGherkin.TestAdapter](https://img.shields.io/nuget/v/NGherkin.TestAdapter?label=NGherkin.TestAdapter)](https://www.nuget.org/packages/NGherkin.TestAdapter)

# Requirements
- NET 6+ (recommended) or .NET Framework 4.6.2+
- Visual Studio 2022 or Visual Studio Code or any other editor with .NET support

Optional: gherking syntax plugin for your code editor:
1) [Reqnroll plugin](https://marketplace.visualstudio.com/items?itemName=Reqnroll.ReqnrollForVisualStudio2022)  for Visual Studio 2022
2) [Cucumber plugin](https://marketplace.visualstudio.com/items?itemName=CucumberOpen.cucumber-official) for Visual Studio Code or any other plugin

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
