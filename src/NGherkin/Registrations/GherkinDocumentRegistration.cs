using Gherkin.Ast;

namespace NGherkin.Registrations;

public sealed class GherkinDocumentRegistration(
    string name,
    GherkinDocument gherkinDocument)
{
    public string Name { get; } = name;
    public GherkinDocument Document { get; } = gherkinDocument;
}
