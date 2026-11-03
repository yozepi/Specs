# Refactoring Plan: Use TestCaseExecutor for VSTest Discovery & Execution

> **Status: COMPLETE — historical record.** This plan has been fully executed. Details that
> have since changed: `TestCaseExecutorSettings.IsCancelled` was removed in favour of a
> `CancellationToken` parameter on `ProcessTestCase`, and `TestInvoker` now delegates suite
> runs to `SelectionTreeWalker`. For the current design see `systemPatterns.md`.

## Background

`TestCaseExecutor` (in `specs.core`) provides a well-designed, adapter-pattern-based test lifecycle executor. Currently, `TestInvoker` (in the VSTest adapter) reimplements the same lifecycle logic manually. Both `TestStructureExtractor` (discovery) and `TestInvoker` (execution) also use reflection to access the private `coreBuilder` field, when `Specifications` already exposes `CoreBuilder` as `internal protected` — accessible via `InternalsVisibleTo`.

---

## Phase 1: Discovery Cleanup — Remove Reflection from TestStructureExtractor ✅

- [x] **File:** `src/specs.BDD.VSTestAdapter/Discovery/TestStructureExtractor.cs`
  1. Remove the `CoreBuilderField` static field (reflection handle)
  2. Replace `CoreBuilderField.GetValue(instance) as IContextualTestCaseBuilder` with `instance.CoreBuilder`
  3. Remove the null-check/exception for `CoreBuilderField == null`
  4. Keep null-check on the builder value itself

**Before:**
```csharp
private static readonly FieldInfo? CoreBuilderField = typeof(Specifications).GetField("coreBuilder", ...);
// ...
if (CoreBuilderField == null) throw ...;
var builder = CoreBuilderField.GetValue(instance) as IContextualTestCaseBuilder ?? throw ...;
```

**After:**
```csharp
var builder = instance.CoreBuilder ?? throw new InvalidOperationException("CoreBuilder is null...");
```

**Tests:** `TestStructureExtractor_Tests` should pass unchanged.

---

## Phase 2: Execution Refactoring

### Step 2a: Remove Reflection from TestInvoker.BuildHierarchy() ✅

- [x] **File:** `src/specs.BDD.VSTestAdapter/Execution/TestInvoker.cs`
  1. Remove `CoreBuilderField` static field
  2. Use `instance.CoreBuilder` directly in `BuildHierarchy()`

### Step 2b: Add Cancellation to TestCaseExecutor ✅

- [x] **File:** `src/specs.core/Framework/Processors/TestExecutors/TestCaseExecutorSettings.cs`
  - Add `Func<bool>? IsCancelled` property
- [x] **File:** `src/specs.core/Framework/Processors/TestExecutors/TestCaseExecutor.cs`
  - Check `settings.IsCancelled?.Invoke()` before each test in the loop

### Step 2c: Create VSTest Adapter Implementations ✅

- [x] **New file:** `src/specs.BDD.VSTestAdapter/Execution/VSTestLoggingAdapter.cs`
  - Implements `ILoggingAdapter`
  - Bridges logging to `IFrameworkHandle` for result reporting
  - Maps test descriptions → `TestCase` objects for `RecordStart`/`RecordResult`
  - Tracks per-test timing

- [x] **New file:** `src/specs.BDD.VSTestAdapter/Execution/VSTestTestProvider.cs`
  - Implements `ITestProviderAdapter`
  - Wraps `(TestCase, Action)` pairs from TestFinder as `TestInstance` objects

### Step 2d: Refactor TestInvoker to Use TestCaseExecutor ✅

- [x] **File:** `src/specs.BDD.VSTestAdapter/Execution/TestInvoker.cs`
  - `InvokeGroup()`: Create adapters → `TestCaseExecutor` → `ProcessTestCase()`
  - `Invoke()`: Same pattern for single-test execution
  - Keep hierarchy build failure handling (reports all tests as failed)

### Step 2e: Update Tests ✅

- [x] All 144 existing tests pass without modification
- [ ] Add tests for new adapter implementations if needed (future)

---

## Design Decisions

### Timing Trade-off
Per-test timing will cover the test assertion only (not SetupEach/Act/TeardownEach). This is acceptable and arguably more accurate.

### Cancellation
Added to `TestCaseExecutorSettings` as `Func<bool>? IsCancelled` — checked before each test in the loop.

### No Reflection Needed
`Specifications.CoreBuilder` is `internal protected` and both `specs.core` and `specs.BDD` have `InternalsVisibleTo("specs.BDD.VSTestAdapter")`.

---

## Files Modified/Created Summary

| File | Action |
|---|---|
| `Discovery/TestStructureExtractor.cs` | Modified — remove reflection |
| `Execution/TestInvoker.cs` | Modified — remove reflection + use TestCaseExecutor |
| `TestExecutors/TestCaseExecutorSettings.cs` | Modified — add cancellation |
| `TestExecutors/TestCaseExecutor.cs` | Modified — check cancellation |
| `Execution/VSTestLoggingAdapter.cs` | **New** |
| `Execution/VSTestTestProvider.cs` | **New** |
