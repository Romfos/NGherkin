using Gherkin.Ast;

namespace NGherkin;

internal sealed record GherkinFeature(
    string Name,
    GherkinDocument Document);