namespace NGherkin.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class GivenAttribute(string pattern) : Attribute
{
    public string Pattern { get; } = pattern;
}