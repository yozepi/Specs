using specs.core.Framework.Adapters;
using System;
using System.Collections.Generic;

namespace specs.core.Framework.Processors.TestCases
{
    /// <summary>
    /// Represents a test case with lifecycle actions and test methods.
    /// Actions execute in order: SetupOnce → SetupEach → Act → Test → TeardownEach → TeardownOnce.
    /// </summary>
    public interface  ITestCase
    {
        /// <summary>
        /// Gets the setup action that runs once before all tests in this context and its children.
        /// Collects actions from parent to child (top-down).
        /// </summary>
        Action SetupOnce { get; }

        /// <summary>
        /// Gets the setup action that runs before each test.
        /// Collects actions from parent to child (top-down).
        /// </summary>
        Action SetupEach { get; }

        /// <summary>
        /// Gets the action under test that runs after setup and before assertions.
        /// Collects actions from parent to child (top-down).
        /// </summary>
        Action Act { get; }

        /// <summary>
        /// Gets the teardown action that runs after each test.
        /// Collects actions from child to parent (bottom-up).
        /// </summary>
        Action TeardownEach { get; }

        /// <summary>
        /// Gets the teardown action that runs once after all tests in this context and its children.
        /// Collects actions from child to parent (bottom-up).
        /// </summary>
        Action TeardownOnce { get; }

        /// <summary>
        /// Gets the setup-once action that belongs exclusively to this node (no ancestor actions included).
        /// Used by the selection-tree walker to fire BeforeAll exactly once per node.
        /// </summary>
        Action OwnSetupOnce { get; }

        /// <summary>
        /// Gets the teardown-once action that belongs exclusively to this node (no ancestor actions included).
        /// Used by the selection-tree walker to fire AfterAll exactly once per node.
        /// </summary>
        Action OwnTeardownOnce { get; }

        /// <summary>
        /// Gets the collection of test methods with their descriptions.
        /// </summary>
        IReadOnlyList<(string description, Action test)> Tests { get; }
    }
}
