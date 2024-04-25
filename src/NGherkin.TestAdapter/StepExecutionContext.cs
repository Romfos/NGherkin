using Gherkin.Ast;
using System.Reflection;

namespace NGherkin.TestAdapter;

internal sealed record StepExecutionContext(
    string ErrorMessageStepText,
    object Service,
    MethodInfo Method,
    List<string> Parameters,
    StepArgument? StepArgument);