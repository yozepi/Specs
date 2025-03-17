using NSpec;
using NSpec.Domain;
using NSpec.Domain.Formatters;
using NUnit.Framework;
using System;
using System.Linq;

namespace yozepi
{
    public abstract class nSpecTestHarness : nspec
    {
        #region properties

        protected internal string Tags { get; set; } = null;
        protected internal IFormatter Formatter { get; set; } = null;
        protected bool FailFast { get; set; }
        internal Func<Type[]> specFinder { get; set; } = null;

        #endregion //properties


        #region constructor

        public nSpecTestHarness()
        {
            this.Formatter = new ConsoleFormatter();
        }

        #endregion //constructor

        #region protected methods

        protected internal void LoadSpecs(Func<Type[]> specFinder)
        {
            this.specFinder = specFinder;
        }

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

        protected internal void RunSpecs(Func<Type[]> specFinder)
        {
            LoadSpecs(specFinder);
            RunSpecs();
        }

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