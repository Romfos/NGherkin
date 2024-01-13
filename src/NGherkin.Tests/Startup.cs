using Gherkin.Ast;
using Microsoft.Extensions.DependencyInjection;

namespace NGherkin.Tests;

internal sealed class Startup : StartupBase
{
    public override void Configure(IServiceCollection services)
    {
        services.AddGherkinFeatures();
        services.AddGherkinSteps();

        services.AddArgumentTransformation(value =>
        {
            if (value is DataTable dataTable
                && dataTable.Rows.Count() > 2
                && dataTable.Rows.First().Cells.Count() == 2)
            {
                return dataTable.Rows.Skip(1).ToDictionary(x => int.Parse(x.Cells.First().Value), x => x.Cells.Last().Value);
            }

            return null;
        });
    }
}
