namespace NGherkin.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class WhenAttribute(string pattern) : Attribute
{
    public string Pattern { get; } = pattern;
}
