using NFluent;
using specs.BDD;
using specs.BDD.Attributes;

namespace specs.BDD.VSTestAdapter.IntegrationTests;

/// <summary>
/// Integration specs for Calculator — demonstrates multi-context discovery
/// and hierarchical test execution via the specs.BDD.VSTestAdapter.
/// </summary>
public class CalculatorSpecs : Specifications
{
    // ─── Context 1: Addition ──────────────────────────────────────────────────

    [Context]
    public void describe_addition()
    {
        Calculator calculator = null!;
        int result = 0;

        BeforeAll(() => { calculator = new Calculator(); });

        Context["when adding two positive numbers"] = () =>
        {
            Act(() => { result = calculator.Add(2, 3); });

            It["returns the correct sum"] = () =>
            {
                Check.That(result).IsEqualTo(5);
            };
        };

        Context["when adding a positive and a negative number"] = () =>
        {
            Act(() => { result = calculator.Add(10, -3); });

            It["returns the net value"] = () =>
            {
                Check.That(result).IsEqualTo(7);
            };
        };

        Context["when adding zero"] = () =>
        {
            Act(() => { result = calculator.Add(42, 0); });

            It["returns the original number"] = () =>
            {
                Check.That(result).IsEqualTo(42);
            };
        };
    }

    // ─── Context 2: Division ─────────────────────────────────────────────────

    [Context]
    public void describe_division()
    {
        Calculator calculator = null!;

        BeforeAll(() => { calculator = new Calculator(); });

        Context["when dividing evenly"] = () =>
        {
            int result = 0;

            Act(() => { result = calculator.Divide(10, 2); });

            It["returns the correct quotient"] = () =>
            {
                Check.That(result).IsEqualTo(5);
            };
        };

        Context["when dividing by zero"] = () =>
        {
            Exception? caught = null;

            Act(() =>
            {
                try { calculator.Divide(10, 0); }
                catch (DivideByZeroException ex) { caught = ex; }
            });

            It["throws DivideByZeroException"] = () =>
            {
                Check.That(caught).IsInstanceOf<DivideByZeroException>();
            };

            It["exception message mentions zero"] = () =>
            {
                Check.That(caught!.Message.ToLowerInvariant()).Contains("zero");
            };
        };
    }

    // ─── Context 3: Multiply (nested contexts) ────────────────────────────────

    [Context]
    public void describe_multiplication()
    {
        Calculator calculator = null!;
        int result = 0;

        BeforeAll(() => { calculator = new Calculator(); });

        Context["positive operands"] = () =>
        {
            Context["small numbers"] = () =>
            {
                Act(() => { result = calculator.Multiply(3, 4); });

                It["returns the correct product"] = () =>
                {
                    Check.That(result).IsEqualTo(12);
                };
            };

            Context["large numbers"] = () =>
            {
                Act(() => { result = calculator.Multiply(100, 200); });

                It["returns the correct product"] = () =>
                {
                    Check.That(result).IsEqualTo(20000);
                };
            };
        };

        Context["when multiplying by zero"] = () =>
        {
            Act(() => { result = calculator.Multiply(99, 0); });

            It["always returns zero"] = () =>
            {
                Check.That(result).IsEqualTo(0);
            };
        };
    }
}
