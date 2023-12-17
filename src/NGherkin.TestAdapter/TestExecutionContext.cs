using Gherkin.Ast;

namespace NGherkin.TestAdapter;

internal sealed class TestExecutionContext(
    Feature feature,
    Scenario scenario,
    KeyValuePair<TableRow, TableRow>? examples,
    List<Step> backgroundSteps)
{
    public Feature Feature { get; } = feature;
    public Scenario Scenario { get; } = scenario;
    public KeyValuePair<TableRow, TableRow>? Examples { get; } = examples;
    public List<Step> BackgroundSteps { get; } = backgroundSteps;
}
