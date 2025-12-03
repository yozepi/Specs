// NFluent and specs imports
using NFluent;
using NUnit.Framework;
using specs.core.Framework.Processors.TestCases;
using yozepi;

namespace specs.BDD.UnitTests
{
    public class AssertBuilderAsync : BaseAssertBuilderSpecs<BDD.AssertBuilderAsync, Func<Task>>
    {
        [Test]
        public void Specs()
        {
            RunMySpecs();
        }

        protected override string ActionVerb => "async";


        protected override BDD.AssertBuilderAsync Construct(IContextualTestCaseBuilder coreBuilder)
        {
            return new BDD.AssertBuilderAsync(coreBuilder);
        }

        protected override Func<Task> EmptyAction() => async () => { await Task.CompletedTask; };

        protected override void ExtendedUsePatterns()
        {
            context["with actual async operations"] = () =>
            {
                it["should handle Task.Delay operations"] = () =>
                {
                    Check.ThatCode(() =>
                    {
                        Subject["should delay"] = async () =>
                        {
                            await Task.Delay(1);
                        };
                    }).DoesNotThrow();
                };

                it["should handle async method calls"] = () =>
                {
                    Check.ThatCode(() =>
                    {
                        Subject["should call async method"] = async () =>
                        {
                            var result = await GetAsyncValue();
                            Check.That(result).IsEqualTo(42);
                        };
                    }).DoesNotThrow();
                };
            };
        }

        private void AssertBuilderAsync_Specs()
        {
            SetSharedSpecs();
        }

        // Helper async method for testing
        private static async Task<int> GetAsyncValue()
        {
            await Task.CompletedTask;
            return 42;
        }
    }
}
