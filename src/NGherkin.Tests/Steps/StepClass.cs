using Gherkin.Ast;
using NGherkin.Attributes;

namespace NGherkin.Tests.Steps;

[Steps]
internal sealed class StepClass
{
    [Given("this is given step")]
    public void Given()
    {
    }

    [When("this is when step with '(.*)' argument an '(.*)' argument")]
    public void When1(int arg1, string arg2, DataTable dataTable)
    {
        if (arg1 is not 1)
        {
            throw new ArgumentException(nameof(arg1));
        }

        if (arg2 is not "text")
        {
            throw new ArgumentException(nameof(arg2));
        }

        if (dataTable is null)
        {
            throw new ArgumentException(nameof(dataTable));
        }
    }

    [When("this is second when step with date '(.*)' argument")]
    public void When2(DateTime dateTime)
    {
        if (dateTime != DateTime.Parse("01/01/2020"))
        {
            throw new ArgumentException(nameof(dateTime));
        }
    }

    [Then("this is then step")]
    public void Then()
    {
    }
}
