using System.Reflection;

namespace NGherkin.TestAdapter;

internal sealed class StepExecutionContext(object target, MethodInfo methodInfo, object[] arguments)
{
    public object Target { get; } = target;
    public MethodInfo MethodInfo { get; } = methodInfo;
    public object[] Arguments { get; } = arguments;
}
