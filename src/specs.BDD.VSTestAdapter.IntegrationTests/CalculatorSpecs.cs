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
                if (result != 5)
                    throw new Exception($"Expected 5 but got {result}");
            };
        };

        Context["when adding a positive and a negative number"] = () =>
        {
            Act(() => { result = calculator.Add(10, -3); });

            It["returns the net value"] = () =>
            {
                if (result != 7)
                    throw new Exception($"Expected 7 but got {result}");
            };
        };

        Context["when adding zero"] = () =>
        {
            Act(() => { result = calculator.Add(42, 0); });

            It["returns the original number"] = () =>
            {
                if (result != 42)
                    throw new Exception($"Expected 42 but got {result}");
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
                if (result != 5)
                    throw new Exception($"Expected 5 but got {result}");
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
                if (caught == null)
                    throw new Exception("Expected DivideByZeroException but none was thrown");
            };

            It["exception message mentions zero"] = () =>
            {
                if (caught?.Message.Contains("zero", StringComparison.OrdinalIgnoreCase) != true)
                    throw new Exception($"Expected message to mention 'zero' but was: '{caught?.Message}'");
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
                    if (result != 12)
                        throw new Exception($"Expected 12 but got {result}");
                };
            };

            Context["large numbers"] = () =>
            {
                Act(() => { result = calculator.Multiply(100, 200); });

                It["returns the correct product"] = () =>
                {
                    if (result != 20000)
                        throw new Exception($"Expected 20000 but got {result}");
                };
            };
        };

        Context["when multiplying by zero"] = () =>
        {
            Act(() => { result = calculator.Multiply(99, 0); });

            It["always returns zero"] = () =>
            {
                if (result != 0)
                    throw new Exception($"Expected 0 but got {result}");
            };
        };
    }
}
