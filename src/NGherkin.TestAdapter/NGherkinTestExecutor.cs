using Gherkin.Ast;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using NGherkin.Registrations;
using System.Text.RegularExpressions;

namespace NGherkin.TestAdapter;

[ExtensionUri(ExecutorUri)]
public sealed class NGherkinTestExecutor : ITestExecutor
{
    public const string ExecutorUri = "executor://NGherkinTestAdapter/v1";

    public void Cancel()
    {
    }

    public void RunTests(IEnumerable<TestCase>? tests, IRunContext? runContext, IFrameworkHandle? frameworkHandle)
    {
        if (tests == null || frameworkHandle == null)
        {
            return;
        }

        var testCasesList = tests.ToList();

        var testCases = testCasesList
            .Select(x => x.Source).Distinct()
            .SelectMany(NGherkinTestDiscoverer.GetTestCases)
            .Where(x => testCasesList.Any(y => x.FullyQualifiedName == y.FullyQualifiedName));

        foreach (var testCase in testCases)
        {
            RunTest(testCase, frameworkHandle);
        }
    }

    public void RunTests(IEnumerable<string>? sources, IRunContext? runContext, IFrameworkHandle? frameworkHandle)
    {
        if (sources == null || frameworkHandle == null)
        {
            return;
        }

        foreach (var test in sources.SelectMany(NGherkinTestDiscoverer.GetTestCases))
        {
            RunTest(test, frameworkHandle);
        }
    }

    private void RunTest(TestCase testCase, IFrameworkHandle frameworkHandle)
    {
        frameworkHandle.RecordStart(testCase);
        var testResult = new TestResult(testCase);

        if (testCase.LocalExtensionData is not TestCaseExecutionContext testCaseExecutionContext)
        {
            throw new Exception("Unable to get test case data");
        }

        using var scope = testCaseExecutionContext.Services.CreateScope();
        var gherkinStepRegistrations = scope.ServiceProvider.GetServices<GherkinStepRegistration>();

        foreach (var step in testCaseExecutionContext.Scenario.Steps)
        {
            var matchedGherkinStepRegistrations = gherkinStepRegistrations
                .Where(x => x.Keyword == step.Keyword.Trim() && x.Pattern.IsMatch(step.Text))
                .ToList();

            if (matchedGherkinStepRegistrations.Count == 0)
            {
                testResult.Outcome = TestOutcome.Failed;
                testResult.ErrorMessage = $"Unable to find step for: {step.Keyword.Trim()} {step.Text}";
                frameworkHandle.RecordResult(testResult);
                frameworkHandle.RecordEnd(testResult.TestCase, testResult.Outcome);
                return;
            }

            if (matchedGherkinStepRegistrations.Count > 1)
            {
                testResult.Outcome = TestOutcome.Failed;
                testResult.ErrorMessage = $"Multiple steps were found for: {step.Keyword.Trim()} {step.Text}";
                frameworkHandle.RecordResult(testResult);
                frameworkHandle.RecordEnd(testResult.TestCase, testResult.Outcome);
                return;
            }

            var gherkinStepRegistration = matchedGherkinStepRegistrations.Single();
            try
            {
                var targetType = scope.ServiceProvider.GetRequiredService(gherkinStepRegistration.Type);

                var parameters = gherkinStepRegistration.Method.GetParameters();

                var stepArguments = gherkinStepRegistration.Pattern
                    .Match(step.Text)
                    .Groups
                    .Cast<Group>()
                    .Skip(1)
                    .Select(x => x.Value)
                    .ToList();

                var expectedParameterCount = step.Argument == null ? stepArguments.Count : stepArguments.Count + 1;
                if (expectedParameterCount != parameters.Length)
                {
                    throw new Exception($"Method {gherkinStepRegistration.Type.FullName}.{gherkinStepRegistration.Method.Name} have invalid parameters count");
                }

                var arguments = stepArguments.Select((value, index) => Convert.ChangeType(value, parameters[index].ParameterType));

                if (step.Argument is DataTable dataTable)
                {
                    arguments = arguments.Concat(new[] { dataTable });
                }

                gherkinStepRegistration.Method.Invoke(targetType, arguments.ToArray());
            }
            catch (Exception exception)
            {
                testResult.Outcome = TestOutcome.Failed;
                testResult.ErrorMessage = exception.InnerException?.Message ?? exception.Message;
                testResult.ErrorStackTrace = exception.InnerException?.StackTrace ?? exception.StackTrace;
                frameworkHandle.RecordResult(testResult);
                frameworkHandle.RecordEnd(testResult.TestCase, testResult.Outcome);
                return;
            }
        }

        testResult.Outcome = TestOutcome.Passed;
        frameworkHandle.RecordResult(testResult);
        frameworkHandle.RecordEnd(testResult.TestCase, testResult.Outcome);
    }
}