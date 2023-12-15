using Gherkin;
using Microsoft.Extensions.DependencyInjection;
using NGherkin.Attributes;
using NGherkin.Registrations;
using System.Reflection;
using System.Text.RegularExpressions;

namespace NGherkin;

public static class ServiceCollectionExtensions
{
    public static void AddGherkinFeatures(this IServiceCollection services)
    {
        var assembly = Assembly.GetCallingAssembly() ?? throw new Exception("Unable to get entry assembly");
        var gherkinParser = new Parser();

        GherkinDocumentRegistration CreateGherkinDocumentRegistration(Assembly assembly, string resourceName)
        {
            using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new Exception($"Unable to get resource {resourceName}");
            using var reader = new StreamReader(stream);
            return new(resourceName, gherkinParser.Parse(reader));
        }

        var gherkinDocuments = assembly.GetManifestResourceNames()
            .Where(resourceName => resourceName.EndsWith(".feature"))
            .Select(resourceName => CreateGherkinDocumentRegistration(assembly, resourceName));

        foreach (var gherkinDocument in gherkinDocuments)
        {
            services.AddSingleton(gherkinDocument);
        }
    }

    public static void AddGherkinSteps(this IServiceCollection services)
    {
        var assembly = Assembly.GetCallingAssembly() ?? throw new Exception("Unable to get entry assembly");
        var stepTypes = assembly.GetTypes().Where(x => x.GetCustomAttribute<StepsAttribute>() != null);

        foreach (var stepType in stepTypes)
        {
            services.AddScoped(stepType);

            foreach (var method in stepType.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                foreach (var attribute in method.GetCustomAttributes<GivenAttribute>())
                {
                    services.AddSingleton(new GherkinStepRegistration(stepType, method, "Given", new Regex(attribute.Pattern)));
                }

                foreach (var attribute in method.GetCustomAttributes<WhenAttribute>())
                {
                    services.AddSingleton(new GherkinStepRegistration(stepType, method, "When", new Regex(attribute.Pattern)));
                }

                foreach (var attribute in method.GetCustomAttributes<ThenAttribute>())
                {
                    services.AddSingleton(new GherkinStepRegistration(stepType, method, "Then", new Regex(attribute.Pattern)));
                }
            }
        }
    }
}
