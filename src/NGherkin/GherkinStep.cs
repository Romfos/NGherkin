using System.Reflection;
using System.Text.RegularExpressions;

namespace NGherkin;

internal sealed record GherkinStep(
    Type ServiceType,
    MethodInfo Method,
    string Keyword,
    Regex Pattern);