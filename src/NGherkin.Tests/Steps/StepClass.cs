using Gherkin.Ast;
using NGherkin.Attributes;

namespace NGherkin.Tests.Steps;

[Steps]
internal sealed class StepClass
{
    [Given("this is given step$")]
    public void Given1()
    {
    }

    [Given("this is given step from background")]
    public void Given2()
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

    [When("this is when step with async task")]
    public async Task When3()
    {
        await Task.Yield();
    }

    [When("this is when step with with argument '(.*)' and value '(.*)'")]
    public void When4(int number, string text)
    {
        if (number is not (1 or 2 or 3 or 4))
        {
            throw new ArgumentException(nameof(number));
        }
        if (text is not ("value1" or "value2" or "value3" or "value4"))
        {
            throw new ArgumentException(nameof(text));
        }
    }

    [Then("this is then step")]
    public void Then()
    {
    }
}
