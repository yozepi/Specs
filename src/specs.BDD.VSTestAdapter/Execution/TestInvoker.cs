using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using specs.BDD.VSTestAdapter.Conversion;
using specs.core.Framework.Processors.TestCases;

namespace specs.BDD.VSTestAdapter.Execution
{
    /// <summary>
    /// Loads the assembly for a <see cref="TestCase"/>, rebuilds the specification's test
    /// hierarchy, locates the specific test, and runs its full lifecycle.
    /// </summary>
    /// <remarks>
    /// For each execution, a fresh <see cref="specs.BDD.Specifications"/> instance is created
    /// and the [Context] method is re-invoked so that lifecycle closures capture a clean scope.
    /// This mirrors the isolation guarantee provided during discovery.
    /// </remarks>
    internal static class TestInvoker
    {
        /// <summary>
        /// Reflection handle for the private <c>coreBuilder</c> field on
        /// <see cref="specs.BDD.Specifications"/>.
        /// </summary>
        private static readonly FieldInfo? CoreBuilderField =
            typeof(specs.BDD.Specifications).GetField(
                "coreBuilder",
                BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Executes a single <see cref="TestCase"/> end-to-end:
        /// resolves the type and method from metadata, rebuilds the hierarchy,
        /// navigates to the target test, runs the full lifecycle, and returns a
        /// <see cref="TestResult"/> with the outcome.
        /// </summary>
        public static TestResult Invoke(TestCase testCase)
        {
            var startTime = DateTimeOffset.UtcNow;
            var timer = Stopwatch.StartNew();

            try
            {
                InvokeCore(testCase);
                timer.Stop();
                return ResultReporter.Success(testCase, startTime, timer.Elapsed);
            }
            catch (Exception ex)
            {
                timer.Stop();
                return ResultReporter.Failure(testCase, ex, startTime, timer.Elapsed);
            }
        }

        // ── Private helpers ────────────────────────────────────────────────────────────────────

        private static void InvokeCore(TestCase testCase)
        {
            // 1. Read the metadata that was embedded during discovery.
            var className = testCase.GetPropertyValue<string>(TestProperties.ClassName, null)
                ?? throw new InvalidOperationException(
                    "TestCase is missing the ClassName metadata property.");

            var methodName = testCase.GetPropertyValue<string>(TestProperties.ContextMethod, null)
                ?? throw new InvalidOperationException(
                    "TestCase is missing the ContextMethod metadata property.");

            var contextPath = testCase.GetPropertyValue<string>(TestProperties.ContextPath, null)
                ?? throw new InvalidOperationException(
                    "TestCase is missing the ContextPath metadata property.");

            var testDescription = testCase.GetPropertyValue<string>(TestProperties.TestDescription, null)
                ?? throw new InvalidOperationException(
                    "TestCase is missing the TestDescription metadata property.");

            // 2. Load the assembly and resolve the specification type.
            //    ClassName is stored as AssemblyQualifiedName; strip assembly-info suffix so
            //    we can use Assembly.GetType() with just the namespace-qualified type name.
            var assembly = Assembly.LoadFrom(testCase.Source);
            var typeName = className.Contains(',') ? className.Split(',')[0].Trim() : className;
            var specType = assembly.GetType(typeName)
                ?? throw new InvalidOperationException(
                    $"Cannot find type '{typeName}' in '{testCase.Source}'.");

            // 3. Resolve the [Context] method.
            var contextMethod = specType.GetMethod(
                    methodName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException(
                    $"Cannot find method '{methodName}' on '{typeName}'.");

            // 4. Instantiate the spec, invoke the [Context] method, and build the hierarchy.
            var root = BuildHierarchy(specType, contextMethod);

            // 5. Navigate to the correct context node and test action.
            var found = TestFinder.Find(root, contextPath, testDescription)
                ?? throw new InvalidOperationException(
                    $"Cannot locate test '{testDescription}' at path '{contextPath}' " +
                    $"in '{typeName}.{methodName}'.");

            // 6. Run the full test lifecycle.
            RunLifecycle(found.Context, found.TestAction);
        }

        /// <summary>
        /// Creates a fresh specification instance, invokes the [Context] method to populate
        /// the builder, then builds and returns the immutable test case hierarchy.
        /// </summary>
        private static IContextualTestCase BuildHierarchy(Type specType, MethodInfo contextMethod)
        {
            if (CoreBuilderField == null)
                throw new InvalidOperationException(
                    "Cannot locate internal 'coreBuilder' field on Specifications. " +
                    "The Specifications class implementation may have changed.");

            var instance = Activator.CreateInstance(specType) as specs.BDD.Specifications
                ?? throw new InvalidOperationException(
                    $"Cannot instantiate specification class '{specType.FullName}'.");

            // Invoke the [Context] method — runs user code that populates the builder.
            // Unwrap TargetInvocationException so any user error surfaces cleanly.
            try
            {
                contextMethod.Invoke(instance, null);
            }
            catch (TargetInvocationException tie) when (tie.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(tie.InnerException).Throw();
                throw; // unreachable; satisfies the compiler
            }

            var builder = CoreBuilderField.GetValue(instance) as IContextualTestCaseBuilder
                ?? throw new InvalidOperationException(
                    "Internal 'coreBuilder' field is null after context method invocation.");

            return builder.Build();
        }

        /// <summary>
        /// Executes the full test lifecycle around a single test action:
        /// SetupOnce → SetupEach → Act → [test] → TeardownEach → TeardownOnce.
        /// Teardown runs even when the test action throws. The exception is re-thrown
        /// with its original stack trace preserved after teardown completes.
        /// </summary>
        private static void RunLifecycle(ITestCase context, Action testAction)
        {
            context.SetupOnce();
            context.SetupEach();
            context.Act();

            // Capture the failure so teardown still runs, then re-throw it afterwards.
            Exception? failure = null;
            try
            {
                testAction.Invoke();
            }
            catch (Exception ex)
            {
                failure = ex;
            }

            context.TeardownEach();
            context.TeardownOnce();

            if (failure != null)
                ExceptionDispatchInfo.Capture(failure).Throw();
        }
    }
}
