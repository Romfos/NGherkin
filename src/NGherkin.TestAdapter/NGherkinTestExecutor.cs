using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using NGherkin.Registrations;

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
            var potentialGherkinStepRegistrations = gherkinStepRegistrations.Where(x => x.Keyword == step.Keyword.Trim() && x.Pattern.IsMatch(step.Text)).ToList();

            if (potentialGherkinStepRegistrations.Count == 0)
            {
                testResult.Outcome = TestOutcome.Failed;
                testResult.ErrorMessage = $"Unable to find step for: {step.Keyword.Trim()} {step.Text}";
                frameworkHandle.RecordResult(testResult);
                frameworkHandle.RecordEnd(testResult.TestCase, testResult.Outcome);
                return;
            }

            if (potentialGherkinStepRegistrations.Count > 1)
            {
                testResult.Outcome = TestOutcome.Failed;
                testResult.ErrorMessage = $"Multiple steps were found for: {step.Keyword.Trim()} {step.Text}";
                frameworkHandle.RecordResult(testResult);
                frameworkHandle.RecordEnd(testResult.TestCase, testResult.Outcome);
                return;
            }

            var gherkinStepRegistration = potentialGherkinStepRegistrations.Single();
            var targetType = scope.ServiceProvider.GetRequiredService(gherkinStepRegistration.Type);

            try
            {
                gherkinStepRegistration.Method.Invoke(targetType, null);
            }
            catch (Exception exception)
            {
                testResult.Outcome = TestOutcome.Failed;
                testResult.ErrorMessage = exception.InnerException!.Message;
                testResult.ErrorStackTrace = exception.InnerException!.StackTrace;
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