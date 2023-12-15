namespace NGherkin.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ThenAttribute(string pattern) : Attribute
{
    public string Pattern { get; } = pattern;
}
