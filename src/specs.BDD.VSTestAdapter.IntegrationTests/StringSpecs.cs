using specs.BDD;
using specs.BDD.Attributes;

namespace specs.BDD.VSTestAdapter.IntegrationTests;

/// <summary>
/// Integration specs for string operations — demonstrates a second Specifications class
/// being discovered alongside CalculatorSpecs in the same assembly.
/// </summary>
public class StringSpecs : Specifications
{
    [Context]
    public void describe_string_operations()
    {
        string input = string.Empty;

        // ── ToUpper ──────────────────────────────────────────────────────────

        Context["ToUpper"] = () =>
        {
            string result = string.Empty;

            Context["with a lowercase string"] = () =>
            {
                BeforeEach(() => { input = "hello"; });
                Act(() => { result = input.ToUpper(); });

                It["converts every character to uppercase"] = () =>
                {
                    if (result != "HELLO")
                        throw new Exception($"Expected 'HELLO' but got '{result}'");
                };
            };

            Context["with an already-uppercase string"] = () =>
            {
                BeforeEach(() => { input = "WORLD"; });
                Act(() => { result = input.ToUpper(); });

                It["returns the string unchanged"] = () =>
                {
                    if (result != "WORLD")
                        throw new Exception($"Expected 'WORLD' but got '{result}'");
                };
            };

            Context["with a mixed-case string"] = () =>
            {
                BeforeEach(() => { input = "HeLLo WoRLd"; });
                Act(() => { result = input.ToUpper(); });

                It["uppercases all characters"] = () =>
                {
                    if (result != "HELLO WORLD")
                        throw new Exception($"Expected 'HELLO WORLD' but got '{result}'");
                };

                It["Calls your mother"] = TODO;
            };

        };

        // ── Contains ─────────────────────────────────────────────────────────

        Context["Contains"] = () =>
        {
            BeforeEach(() => { input = "Hello, World!"; });

            It["finds an existing substring"] = () =>
            {
                if (!input.Contains("World"))
                    throw new Exception("Expected input to contain 'World'");
            };

            It["returns false for a missing substring"] = () =>
            {
                if (input.Contains("xyz"))
                    throw new Exception("Expected input NOT to contain 'xyz'");
            };
        };

        // ── Trim ─────────────────────────────────────────────────────────────

        Context["Trim"] = () =>
        {
            string result = string.Empty;

            BeforeEach(() => { input = "  spaces  "; });
            Act(() => { result = input.Trim(); });

            It["removes leading whitespace"] = () =>
            {
                if (result.StartsWith(" "))
                    throw new Exception("Expected leading spaces to be removed");
            };

            It["removes trailing whitespace"] = () =>
            {
                if (result.EndsWith(" "))
                    throw new Exception("Expected trailing spaces to be removed");
            };

            It["preserves inner content"] = () =>
            {
                if (result != "spaces")
                    throw new Exception($"Expected 'spaces' but got '{result}'");
            };
        };
    }
}
