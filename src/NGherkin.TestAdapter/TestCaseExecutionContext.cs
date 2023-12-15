using Gherkin.Ast;

namespace NGherkin.TestAdapter;

internal sealed class TestCaseExecutionContext(Scenario scenario, IServiceProvider services)
{
    public Scenario Scenario { get; } = scenario;
    public IServiceProvider Services { get; } = services;
}
