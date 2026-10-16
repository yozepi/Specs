using NFluent;
using specs.BDD;
using specs.BDD.Attributes;

namespace specs.BDD.VSTestAdapter.IntegrationTests;

/// <summary>
/// Demonstrates grouped execution: the whole [Context(Grouped = true)] subtree runs as a
/// single test result and the full contextual tree is published to the test's output.
/// </summary>
public class GroupedCalculatorSpecs : Specifications
{
    [Context("subtraction (grouped)", Grouped = true)]
    public void describe_subtraction()
    {
        Calculator calculator = null!;
        int result = 0;

        BeforeAll(() => { calculator = new Calculator(); });

        Context["when subtracting a smaller number"] = () =>
        {
            Act(() => { result = calculator.Add(10, -4); });

            It["returns the positive difference"] = () =>
            {
                Check.That(result).IsEqualTo(6);
            };
        };

        Context["when subtracting a larger number"] = () =>
        {
            Act(() => { result = calculator.Add(4, -10); });

            It["returns a negative difference"] = () =>
            {
                Check.That(result).IsEqualTo(-6);
            };

            It["is symmetric with the positive case"] = () =>
            {
                Check.That(result).IsEqualTo(-6);
            };
        };
    }
}
