using specs.BDD;
using specs.BDD.Attributes;

namespace specs.BDD.VSTestAdapter.IntegrationTests;

/// <summary>
/// Simulates a service that performs async I/O-bound work.
/// Used to demonstrate async Act and ItAsync within the VSTest adapter.
/// </summary>
public class AsyncDataService
{
    public async Task<string> FetchAsync(string key)
    {
        await Task.Delay(1); // simulate I/O latency
        return key switch
        {
            "greeting" => "Hello, World!",
            "empty"    => string.Empty,
            _          => throw new KeyNotFoundException($"Key '{key}' not found.")
        };
    }

    public async Task SaveAsync(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be empty.", nameof(key));
        await Task.Delay(1);
    }
}

/// <summary>
/// Demonstrates async Act, ItAsync, and exception handling patterns
/// with the specs.BDD.VSTestAdapter.
///
/// Key patterns shown:
///  - Act(async () => ...)          — async action under test
///  - ItAsync["..."] = async () =>  — async assertions
///  - Manual exception capture in Act for exception-path testing
/// </summary>
public class AsyncAndExceptionSpecs : Specifications
{
    // ─── Async Act + ItAsync ─────────────────────────────────────────────────

    [Context]
    public void describe_async_fetch()
    {
        AsyncDataService service = null!;
        string result = string.Empty;

        BeforeAll(() => { service = new AsyncDataService(); });

        Context["when fetching an existing key"] = () =>
        {
            Act(async () => { result = await service.FetchAsync("greeting"); });

            It["returns the expected value"] = () =>
            {
                if (result != "Hello, World!")
                    throw new Exception($"Expected 'Hello, World!' but got '{result}'");
            };

            ItAsync["can also assert asynchronously"] = async () =>
            {
                // Re-fetch to demonstrate ItAsync
                var fetched = await service.FetchAsync("greeting");
                if (fetched != "Hello, World!")
                    throw new Exception($"ItAsync: Expected 'Hello, World!' but got '{fetched}'");
            };
        };

        Context["when fetching a key that returns empty string"] = () =>
        {
            Act(async () => { result = await service.FetchAsync("empty"); });

            It["returns an empty string"] = () =>
            {
                if (!string.IsNullOrEmpty(result))
                    throw new Exception($"Expected empty string but got '{result}'");
            };
        };
    }

    // ─── Exception handling in Act ───────────────────────────────────────────

    [Context]
    public void describe_async_exception_handling()
    {
        AsyncDataService service = null!;
        Exception? caughtException = null;

        BeforeAll(() => { service = new AsyncDataService(); });

        Context["when fetching a missing key"] = () =>
        {

            BeforeEach(() => { caughtException = null; });

            Act(async () =>
            {
                try   { await service.FetchAsync("missing-key"); }
                catch (Exception ex) { caughtException = ex; }
            });

            It["throws KeyNotFoundException"] = () =>
            {
                if (caughtException is not KeyNotFoundException)
                    throw new Exception($"Expected KeyNotFoundException but got: {caughtException?.GetType().Name ?? "none"}");
            };

            It["exception message names the missing key"] = () =>
            {
                if (caughtException?.Message.Contains("missing-key") != true)
                    throw new Exception($"Expected message to contain 'missing-key' but was: '{caughtException?.Message}'");
            };
        };

        Context["when saving with an empty key"] = () =>
        {
            BeforeEach(() => { caughtException = null; });

            Act(async () =>
            {
                try   { await service.SaveAsync(string.Empty, "value"); }
                catch (Exception ex) { caughtException = ex; }
            });

            It["throws ArgumentException"] = () =>
            {
                if (caughtException is not ArgumentException)
                    throw new Exception($"Expected ArgumentException but got: {caughtException?.GetType().Name ?? "none"}");
            };
        };
    }

    // ─── Async BeforeAll / AfterAll ──────────────────────────────────────────

    [Context]
    public void describe_async_lifecycle()
    {
        AsyncDataService service = null!;
        bool initialized = false;

        BeforeAll(async () =>
        {
            await Task.Delay(1); // simulate async initialization
            service = new AsyncDataService();
            initialized = true;
        });

        AfterAll(async () =>
        {
            // simulate async teardown (e.g. flushing connections, closing files)
            await Task.Delay(1);
        });

        It["service was initialized asynchronously"] = () =>
        {
            if (!initialized)
                throw new Exception("Expected 'initialized' to be true after async BeforeAll");
        };

        It["service is not null after async BeforeAll"] = () =>
        {
            if (service == null)
                throw new Exception("Expected service to be non-null after async BeforeAll");
        };
    }
}
