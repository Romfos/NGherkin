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
        foreach (var testCase in sources.SelectMany(GetTestCases))
        {
            discoverySink.SendTestCase(testCase);
        }
    }

    internal static IEnumerable<TestCase> GetTestCases(string source)
    {
        var assembly = Assembly.LoadFile(source);
        var services = CreateServiceCollection(assembly).BuildServiceProvider();

        foreach (var gherkinDocumentRegistration in services.GetServices<GherkinDocumentRegistration>())
        {
            foreach (var scenario in gherkinDocumentRegistration.Document.Feature.Children.OfType<Scenario>())
            {
                var testCase = new TestCase()
                {
                    DisplayName = scenario.Name,
                    FullyQualifiedName = $"{gherkinDocumentRegistration.Name}.{gherkinDocumentRegistration.Document.Feature.Name}.{scenario.Name}",
                    ExecutorUri = new Uri(NGherkinTestExecutor.ExecutorUri),
                    Source = source,
                    LocalExtensionData = new TestCaseExecutionContext(scenario, services),
                };

                yield return testCase;
            }
        }
    }

    internal static ServiceCollection CreateServiceCollection(Assembly assembly)
    {
        var services = new ServiceCollection();

        var startupTypes = assembly.GetTypes()
           .Where(x => !x.IsAbstract && x.IsSubclassOf(typeof(StartupBase)))
           .ToList();

        if (startupTypes.Count == 0)
        {
            return services;
        }

        if (startupTypes.Count > 1)
        {
            throw new Exception("Multiple startup classes were declared");
        }

        if (Activator.CreateInstance(startupTypes.Single()) is not StartupBase startup)
        {
            throw new Exception("Unable to create startup class");
        }

        startup.Configure(services);

        return services;
    }
}
