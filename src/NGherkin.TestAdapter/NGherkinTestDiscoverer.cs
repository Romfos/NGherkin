using Gherkin.Ast;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using NGherkin.Registrations;
using System.ComponentModel;
using System.Reflection;

namespace NGherkin.TestAdapter;

[FileExtension(".dll")]
[FileExtension(".exe")]
[DefaultExecutorUri(NGherkinTestExecutor.ExecutorUri)]
[Category("managed")]
public sealed class NGherkinTestDiscoverer : ITestDiscoverer
{
    public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
    {
        foreach (var source in sources)
        {
            try
            {
                var startupType = GetStartupType(source);
                if (startupType == null)
                {
                    continue;
                }
                using var serviceProvider = GetServiceProvider(startupType);

                foreach (var testcase in GetTestCases(source, serviceProvider))
                {
                    discoverySink.SendTestCase(testcase);
                }
            }
            catch (Exception exception)
            {
                logger.SendMessage(TestMessageLevel.Error, exception.ToString());
            }
        }
    }

    internal static Type? GetStartupType(string source)
    {
        var assembly = GetAssembly(source);

        var startupTypes = assembly.GetTypes().Where(x => !x.IsAbstract && x.IsSubclassOf(typeof(StartupBase))).ToList();

        if (startupTypes.Count == 0)
        {
            return null;
        }

        if (startupTypes.Count > 1)
        {
            throw new Exception($"Multiple startup classes were found in {assembly.FullName} assembly");
        }

        return startupTypes.Single();
    }

    internal static ServiceProvider GetServiceProvider(Type startupType)
    {
        var serviceCollection = new ServiceCollection();
        var startup = CreateStartupClass(startupType);

        try
        {
            startup.Configure(serviceCollection);
        }
        catch (Exception exception)
        {
            throw new Exception($"Error during startup configuration {startupType.FullName}", exception);
        }

        try
        {
            return serviceCollection.BuildServiceProvider();
        }
        catch (Exception exception)
        {
            throw new Exception($"Unable to build container for {startupType.FullName}", exception);
        }
    }

    internal static IEnumerable<TestCase> GetTestCases(string source, IServiceProvider serviceProvider)
    {
        foreach (var gherkinDocumentRegistration in serviceProvider.GetServices<GherkinDocumentRegistration>())
        {
            var feature = gherkinDocumentRegistration.Document.Feature;

            foreach (var scenario in feature.Children.OfType<Scenario>())
            {
                if (!scenario.Examples.Any())
                {
                    yield return new TestCase()
                    {
                        DisplayName = scenario.Name,
                        FullyQualifiedName = $"{gherkinDocumentRegistration.Name}.{gherkinDocumentRegistration.Document.Feature.Name}.{scenario.Name}",
                        ExecutorUri = new Uri(NGherkinTestExecutor.ExecutorUri),
                        Source = source,
                        LocalExtensionData = new TestExecutionContext(feature, scenario, null)
                    };
                }

                var caseNumber = 1;
                foreach (var example in scenario.Examples)
                {
                    foreach (var body in example.TableBody)
                    {
                        var testName = $"{scenario.Name}: Example #{caseNumber++}";

                        yield return new TestCase()
                        {
                            DisplayName = testName,
                            FullyQualifiedName = $"{gherkinDocumentRegistration.Name}.{gherkinDocumentRegistration.Document.Feature.Name}.{testName}",
                            ExecutorUri = new Uri(NGherkinTestExecutor.ExecutorUri),
                            Source = source,
                            LocalExtensionData = new TestExecutionContext(feature, scenario, new(example.TableHeader, body))
                        };
                    }
                }
            }
        }
    }

    private static Assembly GetAssembly(string source)
    {
        try
        {
            return Assembly.LoadFrom(source);
        }
        catch (Exception exception)
        {
            throw new Exception($"Unable to load assembly from source {source}", exception);
        }
    }

    private static StartupBase CreateStartupClass(Type startupType)
    {
        try
        {
            if (Activator.CreateInstance(startupType) is not StartupBase startup)
            {
                throw new Exception($"Unable to create startup class from {startupType.FullName}");
            }

            return startup;
        }
        catch (Exception exception)
        {
            throw new Exception($"Unable to create startup class from {startupType.FullName}", exception);
        }
    }
}
