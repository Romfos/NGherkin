using Gherkin.Ast;

namespace NGherkin.TestAdapter;

internal sealed record TestExecutionContext(
    Feature Feature,
    Scenario Scenario,
    KeyValuePair<TableRow, TableRow>? Examples,
    List<Step> BackgroundSteps);