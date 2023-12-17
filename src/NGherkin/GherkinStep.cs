using System.Reflection;
using System.Text.RegularExpressions;

namespace NGherkin;

public sealed class GherkinStep(
    Type serviceType,
    MethodInfo method,
    string keyword,
    Regex pattern)
{
    public Type ServiceType { get; } = serviceType;
    public MethodInfo Method { get; } = method;
    public string Keyword { get; } = keyword;
    public Regex Pattern { get; } = pattern;
}
