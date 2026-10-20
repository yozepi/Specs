using NFluent;
using specs.BDD;
using specs.BDD.Attributes;
using System.Diagnostics;

namespace specs.BDD.VSTestAdapter.IntegrationTests;

/// <summary>
/// Integration specs for Calculator — demonstrates multi-context discovery
/// and hierarchical test execution via the specs.BDD.VSTestAdapter.
/// </summary>
[TestStructure(TestStructure.PerSpec)]
public class CalculatorSpecs : Specifications
{
    Calculator calculator = null!;
    int num1 = 0;
    int num2 = 0;
    int? result = null;

    public CalculatorSpecs()
    {
        BeforeAll(() => calculator = new Calculator());
        BeforeEach(() =>
        {
            num1 = num2 = 0;
            result = null;
        });
    }


    // ─── Context 1: Addition ──────────────────────────────────────────────────

    [Describe("Addition")]
    public void describe_addition()
    {
        Act(() => { result = calculator.Add(num1, num2); });

        Context["when adding two positive numbers"] = () =>
        {

            BeforeEach(() => { num1 = 2; num2 = 3; });
            It["returns the correct sum"] = () =>
            {
                Check.That(result).IsEqualTo(5);
            };
        };

        Context["when adding a positive and a negative number"] = () =>
        {
            BeforeEach(() => { num1 = 10; num2 = -3; });
            It["returns the net value"] = () =>
            {
                Check.That(result).IsEqualTo(7);
            };
        };

        Context["when adding zero"] = () =>
        {
            BeforeEach(() => { num1 = 42; num2 = 0; });
            It["returns the original number"] = () =>
            {
                Check.That(result).IsEqualTo(42);
            };
        };
    }

    // ─── Context 2: Division ─────────────────────────────────────────────────

    [Describe("Division")]
    public void describe_division()
    {
        Act(() => { result = calculator.Divide(num1, num2); });

        Context["when dividing evenly"] = () =>
        {
            BeforeEach(() => { num1 = 10; num2 = 2; });

            It["returns the correct quotient"] = () =>
            {
                Check.That(result).IsEqualTo(5);
            };
        };

        Context["when dividing by zero"] = () =>
        {
            It["throws DivideByZeroException"] = ShouldThrow<DivideByZeroException>();

            It["exception message mentions zero"] = ShouldThrow<DivideByZeroException>(ex =>
            {
                Check.That(ex.Message.ToLowerInvariant()).Contains("zero");
            });
        };
    }

    // ─── Context 3: Multiply (nested contexts) ────────────────────────────────

    [Describe("Multiplication")]
    public void describe_multiplication()
    { 
        Act(() => { result = calculator.Multiply(num1, num2); });

        Context["positive operands"] = () =>
        {
            Context["small numbers"] = () =>
            {
                BeforeEach(() => { num1 = 3; num2 = 4; });

                It["returns the correct product"] = () =>
                {
                    Check.That(result).IsEqualTo(12);
                };
            };

            Context["large numbers"] = () =>
            {
                BeforeEach(() => { num1 = 100; num2 = 200; });

                It["returns the correct product"] = () =>
                {
                    Check.That(result).IsEqualTo(20000);
                };
            };
        };

        Context["when multiplying by zero"] = () =>
        {
            BeforeEach(() => { num1 = 99; num2 = 0; });

            It["always returns zero"] = () =>
            {
                Check.That(result).IsEqualTo(0);
            };
        };
    }
    
    [Describe("Subtraction (grouped)", Structure = TestStructure.PerDescribe)]
    public void describe_subtraction()
    {
        Act(() => { result = calculator.Subtract(num1, num2); });

        Context["when subtracting a smaller number"] = () =>
        {
            BeforeEach(() => { num1 = 10; num2 = 4; });

            It["returns the positive difference"] = () =>
            {
                Check.That(result).IsEqualTo(6);
            };
        };

        Context["when subtracting a larger number"] = () =>
        {
            BeforeEach(() => { num1 = 4; num2 = 10; });

            It["returns a negative difference"] = () =>
            {
                Check.That(result).IsEqualTo(-6);
            };
        };
    }
}
