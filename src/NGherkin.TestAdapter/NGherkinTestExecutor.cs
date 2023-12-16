using Gherkin.Ast;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using NGherkin.Registrations;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        var startTime = DateTime.Now;
        frameworkHandle.RecordStart(testCase);
        var testResult = new TestResult(testCase);

        if (testCase.LocalExtensionData is not TestCaseExecutionContext testCaseExecutionContext)
        {
            throw new Exception("Unable to get test case data");
        }

        using var scope = testCaseExecutionContext.Services.CreateScope();
        var gherkinStepRegistrations = scope.ServiceProvider.GetServices<GherkinStepRegistration>();

        try
        {
            var stepExecutionContexts = testCaseExecutionContext.Scenario.Steps
                .Select(step => GetStepExecutionContext(step, scope.ServiceProvider, gherkinStepRegistrations))
                .ToList();

            foreach (var stepExecutionContext in stepExecutionContexts)
            {
                var result = stepExecutionContext.MethodInfo.Invoke(stepExecutionContext.Target, stepExecutionContext.Arguments);
                AwaitIfRequired(result);
            }
        }
        catch (Exception exception)
        {
            testResult.Outcome = TestOutcome.Failed;
            testResult.Duration = DateTime.Now - startTime;
            testResult.ErrorMessage = exception.ToString();
            testResult.ErrorStackTrace = exception.StackTrace;
            frameworkHandle.RecordResult(testResult);
            frameworkHandle.RecordEnd(testResult.TestCase, testResult.Outcome);
            return;
        }

        testResult.Outcome = TestOutcome.Passed;
        testResult.Duration = DateTime.Now - startTime;
        frameworkHandle.RecordResult(testResult);
        frameworkHandle.RecordEnd(testResult.TestCase, testResult.Outcome);
    }

    private void AwaitIfRequired(object? result)
    {
        if (result?.GetType().GetMethod(nameof(Task.GetAwaiter)) is MethodInfo getAwaiter)
        {
            var awaiter = getAwaiter.Invoke(result, null);
            if (awaiter?.GetType().GetMethod(nameof(TaskAwaiter.GetResult)) is MethodInfo getResult)
            {
                getResult.Invoke(awaiter, null);
            }
        }
    }

    private StepExecutionContext GetStepExecutionContext(
        Step step,
        IServiceProvider testScopedServiceProvider,
        IEnumerable<GherkinStepRegistration> gherkinStepRegistrations)
    {
        var matchedGherkinStepRegistrations = gherkinStepRegistrations
            .Where(x => x.Keyword == step.Keyword.Trim() && x.Pattern.IsMatch(step.Text))
            .ToList();

        if (matchedGherkinStepRegistrations.Count == 0)
        {
            throw new Exception($"Unable to find step for: {step.Keyword.Trim()} {step.Text}");
        }

        if (matchedGherkinStepRegistrations.Count > 1)
        {
            throw new Exception($"Multiple steps were found for: {step.Keyword.Trim()} {step.Text}");
        }

        var gherkinStepRegistration = matchedGherkinStepRegistrations.Single();
        var target = testScopedServiceProvider.GetRequiredService(gherkinStepRegistration.Type);
        var arguments = ParseStepArguments(gherkinStepRegistration, step);

        return new StepExecutionContext(target, gherkinStepRegistration.Method, arguments);
    }

    private object[] ParseStepArguments(GherkinStepRegistration gherkinStepRegistration, Step step)
    {
        var stepTextArguments = gherkinStepRegistration.Pattern
            .Match(step.Text)
            .Groups
            .Cast<Group>()
            .Skip(1)
            .Select(x => x.Value)
            .ToList();

        var parameters = gherkinStepRegistration.Method.GetParameters();

        var expectedParameterCount = step.Argument == null ? stepTextArguments.Count : stepTextArguments.Count + 1;
        if (expectedParameterCount != parameters.Length)
        {
            throw new Exception($"Method {gherkinStepRegistration.Type.FullName}.{gherkinStepRegistration.Method.Name} have invalid parameters count");
        }

        var arguments = stepTextArguments.Select((value, index) => Convert.ChangeType(value, parameters[index].ParameterType));
        if (step.Argument is DataTable dataTable)
        {
            arguments = arguments.Concat([dataTable]);
        }

        try
        {
            return arguments.ToArray();
        }
        catch (Exception exception)
        {
            throw new Exception($"Unable to parse arguments for step: {step.Keyword.Trim()} {step.Text}", exception);
        }
    }
}