using NSpec;
using NSpec.Domain;
using NSpec.Domain.Formatters;
using NUnit.Framework;
using System;
using System.Linq;

namespace yozepi
{
    /// <summary>
    /// Base class for NSpec test harness integration.
    /// Provides methods to discover, load, and execute NSpec-style tests with NUnit integration.
    /// </summary>
    public abstract class nSpecTestHarness : nspec
    {
        #region properties

        /// <summary>
        /// Gets or sets the tags used to filter which tests to run.
        /// </summary>
        protected internal string Tags { get; set; } = null;
        
        /// <summary>
        /// Gets or sets the formatter used to display test results.
        /// </summary>
        protected internal IFormatter Formatter { get; set; } = null;
        
        /// <summary>
        /// Gets or sets a value indicating whether to stop execution on the first test failure.
        /// </summary>
        protected bool FailFast { get; set; }
        
        /// <summary>
        /// Gets or sets the function used to discover specification types.
        /// </summary>
        internal Func<Type[]> specFinder { get; set; } = null;

        #endregion //properties


        #region constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="nSpecTestHarness"/> class.
        /// Sets the default formatter to <see cref="ConsoleFormatter"/>.
        /// </summary>
        public nSpecTestHarness()
        {
            this.Formatter = new ConsoleFormatter();
        }

        #endregion //constructor

        #region protected methods

        /// <summary>
        /// Loads specifications from the provided type finder function.
        /// </summary>
        /// <param name="specFinder">A function that returns the specification types to test.</param>
        protected internal void LoadSpecs(Func<Type[]> specFinder)
        {
            this.specFinder = specFinder;
        }

        /// <summary>
        /// Runs the loaded specifications using the NSpec test runner.
        /// Asserts that no failures occur and handles pending/focused tests.
        /// </summary>
        protected internal void RunSpecs()
        {
            var types = FindSpecTypes();
            var finder = new SpecFinder(types, "");
            var tagsFilter = new Tags().Parse(Tags);
            var builder = new ContextBuilder(finder, tagsFilter, new DefaultConventions());
            var runner = new ContextRunner(tagsFilter, Formatter!, FailFast);
            var contexts = builder.Contexts().Build();

            bool hasFocusTags = contexts.AnyTaggedWithFocus();
            if (hasFocusTags)
            {
                tagsFilter = new Tags().Parse(NSpec.Domain.Tags.Focus);

                builder = new ContextBuilder(finder, tagsFilter, new DefaultConventions());

                runner = new ContextRunner(tagsFilter, Formatter, FailFast);

                contexts = builder.Contexts().Build();
            }


            var results = runner.Run(contexts);

            //assert that there aren't any failures
            Assert.AreEqual(0, results.Failures().Count());

            var pending = results.Examples().Count(xmpl => xmpl.Pending);
            if (pending != 0)
            {
                Assert.Inconclusive("{0} spec(s) are marked as pending.", pending);
            }
            if (results.Examples().Count() == 0)
            {
                Assert.Inconclusive("Spec count is zero.");
            }
            if (hasFocusTags)
            {
                Assert.Inconclusive("One or more specs are tagged with focus.");
            }
        }

        /// <summary>
        /// Loads and runs specifications in a single operation.
        /// </summary>
        /// <param name="specFinder">A function that returns the specification types to test.</param>
        protected internal void RunSpecs(Func<Type[]> specFinder)
        {
            LoadSpecs(specFinder);
            RunSpecs();
        }

        /// <summary>
        /// Runs only the specifications defined in the current test class.
        /// </summary>
        protected internal void RunMySpecs()
        {
            LoadSpecs(() => new Type[] { this.GetType() });
            RunSpecs();
        }

        #endregion //protected methods

        #region helpers

        private Type[] FindSpecTypes()
        {
            if (specFinder != null)
            {
                return specFinder();
            }
            return GetType().Assembly.GetTypes();
        }

        #endregion //helpers
    }
}
