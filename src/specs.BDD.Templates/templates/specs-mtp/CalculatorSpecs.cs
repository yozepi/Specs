using NFluent;
using specs.BDD;
using specs.BDD.Attributes;

namespace Specs.Template;

public class CalculatorSpecs : Specifications
{
    int num1;
    int num2;
    int result;

    public CalculatorSpecs()
    {
        BeforeEach(() => num1 = num2 = result = 0);
    }

    [Feature("Addition")]
    public void describe_addition()
    {
        Act(() => result = num1 + num2);

        Context["when adding two positive numbers"] = () =>
        {
            BeforeEach(() => { num1 = 2; num2 = 3; });

            It["returns the sum"] = () => Check.That(result).IsEqualTo(5);
        };
    }
}
