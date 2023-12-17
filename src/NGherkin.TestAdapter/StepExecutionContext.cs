using Gherkin.Ast;
using System.Reflection;

namespace NGherkin.TestAdapter;

internal sealed class StepExecutionContext(
    string errorMessageStepText,
    object service,
    MethodInfo method,
    List<string> parameters,
    StepArgument? stepArgument)
{
    public string ErrorMessageStepText { get; } = errorMessageStepText;
    public object Service { get; } = service;
    public MethodInfo Method { get; } = method;
    public List<string> Parameters { get; } = parameters;
    public StepArgument? StepArgument { get; } = stepArgument;
}
