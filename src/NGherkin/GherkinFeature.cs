using Gherkin.Ast;

namespace NGherkin;

internal sealed class GherkinFeature(
    string name,
    GherkinDocument gherkinDocument)
{
    public string Name { get; } = name;
    public GherkinDocument Document { get; } = gherkinDocument;
}
