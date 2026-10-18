using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace specs.BDD.VSTestAdapter.Conversion
{
    /// <summary>
    /// Custom <see cref="TestProperty"/> registrations for metadata that the adapter stores on each
    /// <see cref="TestCase"/> during discovery and reads back during execution.
    /// </summary>
    public static class TestProperties
    {
        /// <summary>
        /// The fully-qualified name of the Specification class that contains the test.
        /// Stored as a string. Used during execution to instantiate the correct class.
        /// </summary>
        public static readonly TestProperty ClassName =
            TestProperty.Register(
                id: "SpecsBDD.ClassName",
                label: "Class Name",
                valueType: typeof(string),
                owner: typeof(TestProperties));

        /// <summary>
        /// The name of the [Describe]-decorated method that builds the test structure.
        /// Stored as a string. Used during execution to invoke the correct method.
        /// </summary>
        public static readonly TestProperty ContextMethod =
            TestProperty.Register(
                id: "SpecsBDD.ContextMethod",
                label: "Context Method",
                valueType: typeof(string),
                owner: typeof(TestProperties));

        /// <summary>
        /// The context path to the test's parent node in the hierarchy
        /// (e.g., "describe_addition → when adding positive numbers").
        /// Stored as a string. Used during execution to locate the right context in the tree.
        /// </summary>
        public static readonly TestProperty ContextPath =
            TestProperty.Register(
                id: "SpecsBDD.ContextPath",
                label: "Context Path",
                valueType: typeof(string),
                owner: typeof(TestProperties));

        /// <summary>
        /// The description of the specific It[...] assertion
        /// (e.g., "should return correct sum").
        /// Stored as a string. Used during execution to locate the right test within its context.
        /// </summary>
        public static readonly TestProperty TestDescription =
            TestProperty.Register(
                id: "SpecsBDD.TestDescription",
                label: "Test Description",
                valueType: typeof(string),
                owner: typeof(TestProperties));

        /// <summary>
        /// Whether this test case represents an entire root [Describe] subtree executed as a
        /// single grouped result. Stored as a bool. Used during execution to route the test
        /// case to the grouped-context runner instead of the per-test selection walker.
        /// </summary>
        public static readonly TestProperty IsGrouped =
            TestProperty.Register(
                id: "SpecsBDD.IsGrouped",
                label: "Is Grouped",
                valueType: typeof(bool),
                owner: typeof(TestProperties));

        /// <summary>
        /// Whether the spec (or, for a grouped test case, every spec in the context) is pending.
        /// Stored as a bool at discovery time. Exposed to <c>dotnet test --filter IsPending=true|false</c>.
        /// </summary>
        public static readonly TestProperty IsPending =
            TestProperty.Register(
                id: "SpecsBDD.IsPending",
                label: "Is Pending",
                valueType: typeof(bool),
                owner: typeof(TestProperties));
    }
}
