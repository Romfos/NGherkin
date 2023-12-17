using Gherkin.Ast;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
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
        foreach (var gherkinFeature in serviceProvider.GetServices<GherkinFeature>())
        {
            var feature = gherkinFeature.Document.Feature;

            var featureBackgroundSteps = GetFeatureBackgroundSteps(feature);

            foreach (var rule in feature.Children.OfType<Rule>())
            {
                var ruleBackgroundSteps = featureBackgroundSteps.Concat(GetRuleBackgroundSteps(rule)).ToList();

                foreach (var scenario in rule.Children.OfType<Scenario>().Where(x => !x.Examples.Any()))
                {
                    var testCase = new TestCase()
                    {
                        DisplayName = scenario.Name,
                        FullyQualifiedName = $"{gherkinFeature.Name}.{gherkinFeature.Document.Feature.Name}.{rule.Name}.{scenario.Name}",
                        ExecutorUri = new Uri(NGherkinTestExecutor.ExecutorUri),
                        Source = source,
                        LocalExtensionData = new TestExecutionContext(feature, scenario, null, ruleBackgroundSteps)
                    };

                    testCase.Traits.AddRange(feature.Tags.Concat(scenario.Tags).Select(x => new Trait(x.Name, string.Empty)));

                    yield return testCase;
                }

                foreach (var scenario in rule.Children.OfType<Scenario>().Where(x => x.Examples.Any()))
                {
                    var scenarioCaseNumber = 1;

                    foreach (var example in scenario.Examples)
                    {
                        foreach (var body in example.TableBody)
                        {
                            var testName = $"{scenario.Name}: Example #{scenarioCaseNumber++}";

                            var testCase = new TestCase()
                            {
                                DisplayName = testName,
                                FullyQualifiedName = $"{gherkinFeature.Name}.{gherkinFeature.Document.Feature.Name}.{rule.Name}.{testName}",
                                ExecutorUri = new Uri(NGherkinTestExecutor.ExecutorUri),
                                Source = source,
                                LocalExtensionData = new TestExecutionContext(feature, scenario, new(example.TableHeader, body), ruleBackgroundSteps)
                            };

                            testCase.Traits.AddRange(feature.Tags.Concat(rule.Tags).Concat(scenario.Tags).Select(x => new Trait(x.Name, string.Empty)));

                            yield return testCase;
                        }
                    }
                }
            }

            foreach (var scenario in feature.Children.OfType<Scenario>().Where(x => !x.Examples.Any()))
            {
                var testCase = new TestCase()
                {
                    DisplayName = scenario.Name,
                    FullyQualifiedName = $"{gherkinFeature.Name}.{gherkinFeature.Document.Feature.Name}.{scenario.Name}",
                    ExecutorUri = new Uri(NGherkinTestExecutor.ExecutorUri),
                    Source = source,
                    LocalExtensionData = new TestExecutionContext(feature, scenario, null, featureBackgroundSteps)
                };

                testCase.Traits.AddRange(feature.Tags.Concat(scenario.Tags).Select(x => new Trait(x.Name, string.Empty)));

                yield return testCase;
            }

            foreach (var scenario in feature.Children.OfType<Scenario>().Where(x => x.Examples.Any()))
            {
                var scenarioCaseNumber = 1;

                foreach (var example in scenario.Examples)
                {
                    foreach (var body in example.TableBody)
                    {
                        var testName = $"{scenario.Name}: Example #{scenarioCaseNumber++}";

                        var testCase = new TestCase()
                        {
                            DisplayName = testName,
                            FullyQualifiedName = $"{gherkinFeature.Name}.{gherkinFeature.Document.Feature.Name}.{testName}",
                            ExecutorUri = new Uri(NGherkinTestExecutor.ExecutorUri),
                            Source = source,
                            LocalExtensionData = new TestExecutionContext(feature, scenario, new(example.TableHeader, body), featureBackgroundSteps)
                        };

                        testCase.Traits.AddRange(feature.Tags.Concat(scenario.Tags).Select(x => new Trait(x.Name, string.Empty)));

                        yield return testCase;
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

    private static List<Step> GetFeatureBackgroundSteps(Feature feature)
    {
        var backgrounds = feature.Children.OfType<Background>().ToList();
        if (backgrounds.Count > 1)
        {
            throw new Exception("Multiple backgrounds are not supported for feature");
        }

        return backgrounds.SelectMany(x => x.Steps).ToList();
    }

    private static IEnumerable<Step> GetRuleBackgroundSteps(Rule rule)
    {
        var backgrounds = rule.Children.OfType<Background>().ToList();
        if (backgrounds.Count > 1)
        {
            throw new Exception("Multiple backgrounds are not supported for rule");
        }

        return backgrounds.SelectMany(x => x.Steps);
    }
}
