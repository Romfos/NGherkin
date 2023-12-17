using Gherkin.Ast;

namespace NGherkin.TestAdapter;

internal sealed class TestExecutionContext(
    Feature feature,
    Scenario scenario)
{
    public Feature Feature { get; } = feature;
    public Scenario Scenario { get; } = scenario;
}
