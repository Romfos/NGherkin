using System.Reflection;
using System.Text.RegularExpressions;

namespace NGherkin.Registrations;

public sealed class GherkinStepRegistration(Type type, MethodInfo method, string keyword, Regex pattern)
{
    public Type Type { get; } = type;
    public MethodInfo Method { get; } = method;
    public string Keyword { get; } = keyword;
    public Regex Pattern { get; } = pattern;
}
