using Gherkin.Ast;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System.Reflection;
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

        var testNames = tests.Select(x => x.FullyQualifiedName).ToList();

        foreach (var source in tests.Select(x => x.Source).Distinct())
        {
            try
            {
                var startupType = NGherkinTestDiscoverer.GetStartupType(source);
                if (startupType == null)
                {
                    continue;
                }

                using var serviceProvider = NGherkinTestDiscoverer.GetServiceProvider(startupType);
                var gherkinStep = serviceProvider.GetServices<GherkinStep>();

                foreach (var testCase in NGherkinTestDiscoverer.GetTestCases(source, serviceProvider))
                {
                    if (testNames.Contains(testCase.FullyQualifiedName))
                    {
                        using var scopedServiceProvider = serviceProvider.CreateScope();
                        RunTest(frameworkHandle, scopedServiceProvider.ServiceProvider, gherkinStep, testCase);
                    }
                }
            }
            catch (Exception exception)
            {
                frameworkHandle.SendMessage(TestMessageLevel.Error, exception.ToString());
            }
        }
    }

    public void RunTests(IEnumerable<string>? sources, IRunContext? runContext, IFrameworkHandle? frameworkHandle)
    {
        if (sources == null || frameworkHandle == null)
        {
            return;
        }

        foreach (var source in sources)
        {
            try
            {
                var startupType = NGherkinTestDiscoverer.GetStartupType(source);
                if (startupType == null)
                {
                    continue;
                }

                using var serviceProvider = NGherkinTestDiscoverer.GetServiceProvider(startupType);
                var gherkinStep = serviceProvider.GetServices<GherkinStep>();

                foreach (var testCase in NGherkinTestDiscoverer.GetTestCases(source, serviceProvider))
                {
                    using var scopedServiceProvider = serviceProvider.CreateScope();
                    RunTest(frameworkHandle, scopedServiceProvider.ServiceProvider, gherkinStep, testCase);
                }
            }
            catch (Exception exception)
            {
                frameworkHandle.SendMessage(TestMessageLevel.Error, exception.ToString());
            }
        }
    }

    private void RunTest(
        IFrameworkHandle frameworkHandle,
        IServiceProvider serviceProvider,
        IEnumerable<GherkinStep> gherkinSteps,
        TestCase testCase)
    {
        frameworkHandle.RecordStart(testCase);
        var testResult = new TestResult(testCase);
        testResult.StartTime = DateTime.Now;

        if (testCase.LocalExtensionData is not TestExecutionContext testExecutionContext)
        {
            throw new Exception($"Unable to get {nameof(TestExecutionContext)}");
        }

        try
        {
            var stepExecutionContexts = GetStepExecutionContexts(serviceProvider, gherkinSteps, testExecutionContext).ToList();

            foreach (var stepExecutionContext in stepExecutionContexts)
            {
                RunTestStep(stepExecutionContext);
            }

            testResult.Outcome = TestOutcome.Passed;
        }
        catch (Exception exception)
        {
            testResult.ErrorMessage = exception.ToString();
            testResult.ErrorStackTrace = exception.InnerException?.StackTrace ?? exception.StackTrace;
            testResult.Outcome = TestOutcome.Failed;
        }

        testResult.EndTime = DateTime.Now;
        testResult.Duration = testResult.EndTime - testResult.StartTime;
        frameworkHandle.RecordResult(testResult);
        frameworkHandle.RecordEnd(testCase, testResult.Outcome);
    }

    private IEnumerable<StepExecutionContext> GetStepExecutionContexts(
        IServiceProvider serviceProvider,
        IEnumerable<GherkinStep> gherkinSteps,
        TestExecutionContext testExecutionContext)
    {
        var backgroundStepKeyword = "Given";
        foreach (var step in testExecutionContext.BackgroundSteps)
        {
            backgroundStepKeyword = GetRealKeyword(step, backgroundStepKeyword);
            var stepText = step.Text;
            yield return GetStepExecutionContext(serviceProvider, gherkinSteps, backgroundStepKeyword, stepText, step.Argument);
        }

        var keyword = "Given";
        foreach (var step in testExecutionContext.Scenario.Steps)
        {
            keyword = GetRealKeyword(step, keyword);
            var stepText = GetRealStepText(step, testExecutionContext);
            yield return GetStepExecutionContext(serviceProvider, gherkinSteps, keyword, stepText, step.Argument);
        }
    }

    private StepExecutionContext GetStepExecutionContext(
        IServiceProvider serviceProvider,
        IEnumerable<GherkinStep> gherkinSteps,
        string keyword,
        string stepText,
        StepArgument stepArgument)
    {
        var errorMessageStepText = $"{keyword} {stepText}";

        var matchedGherkinSteps = gherkinSteps
            .Where(x => x.Keyword == keyword && x.Pattern.IsMatch(stepText))
            .ToList();

        if (matchedGherkinSteps.Count == 0)
        {
            throw new Exception($"Unable to find step implementation for: {errorMessageStepText}");
        }

        if (matchedGherkinSteps.Count > 1)
        {
            throw new Exception($"Multiple step implementations were found for: {errorMessageStepText}");
        }

        var matchedGherkinStep = matchedGherkinSteps.Single();

        var service = serviceProvider.GetRequiredService(matchedGherkinStep.ServiceType);

        var parameters = matchedGherkinStep.Pattern
            .Match(stepText)
            .Groups
            .Cast<Group>()
            .Skip(1)
            .Select(x => x.Value)
            .ToList();

        var expectedParametersCount = stepArgument != null ? parameters.Count + 1 : parameters.Count;

        if (matchedGherkinStep.Method.GetParameters().Length != expectedParametersCount)
        {
            throw new Exception($"Invalid parameter count for {matchedGherkinStep.ServiceType.FullName}.{matchedGherkinStep.Method}");
        }

        var stepExecutionContext = new StepExecutionContext(
            errorMessageStepText,
            service,
            matchedGherkinStep.Method,
            parameters,
            stepArgument);

        return stepExecutionContext;
    }

    private void RunTestStep(StepExecutionContext stepExecutionContext)
    {
        var arguments = ParseStepArguments(stepExecutionContext);

        var result = stepExecutionContext.Method.Invoke(stepExecutionContext.Service, arguments);
        if (result?.GetType().GetMethod("GetAwaiter") is MethodInfo getAwaiter)
        {
            var awaiter = getAwaiter.Invoke(result, null);
            if (awaiter?.GetType().GetMethod("GetResult") is MethodInfo getResult)
            {
                getResult.Invoke(awaiter, null);
            }
        }
    }

    private string GetRealKeyword(Step step, string previousKeyword)
    {
        var stepKeyword = step.Keyword.Trim();
        return stepKeyword == "And" ? previousKeyword : stepKeyword;
    }

    private string GetRealStepText(Step step, TestExecutionContext testExecutionContext)
    {
        var stepText = step.Text;

        if (testExecutionContext.Examples != null)
        {
            var headers = testExecutionContext.Examples.Value.Key.Cells.Select(x => x.Value).ToList();
            var values = testExecutionContext.Examples.Value.Value.Cells.Select(x => x.Value).ToList();

            if (headers.Count != values.Count)
            {
                throw new Exception("Number of headers in examples should match number of values");
            }

            for (var i = 0; i < headers.Count; i++)
            {
                stepText = stepText.Replace($"<{headers[i]}>", values[i]);
            }
        }

        return stepText;
    }

    private object[] ParseStepArguments(StepExecutionContext stepExecutionContext)
    {
        try
        {
            var parameters = stepExecutionContext.Method.GetParameters();
            var arguments = stepExecutionContext.Parameters.Select((value, index) => Convert.ChangeType(value, parameters[index].ParameterType));
            if (stepExecutionContext.StepArgument != null)
            {
                arguments = arguments.Concat([stepExecutionContext.StepArgument]);
            }
            return arguments.ToArray();
        }
        catch (Exception exception)
        {
            throw new Exception($"Unable to parse arguments for step: {stepExecutionContext.ErrorMessageStepText}", exception);
        }
    }
}