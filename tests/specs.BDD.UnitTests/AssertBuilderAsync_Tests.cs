using NFluent;
using specs.BDD.Attributes;
using specs.core.Framework.Processors.TestCases;

namespace specs.BDD.UnitTests
{
    public class AssertBuilderAsync_Tests : BaseAssertBuilderSpecs<BDD.AssertBuilderAsync, Func<Task>>
    {
        protected override string ActionVerb => "async";


        protected override BDD.AssertBuilderAsync Construct(IContextualTestCaseBuilder coreBuilder)
        {
            return new BDD.AssertBuilderAsync(coreBuilder);
        }

        protected override Func<Task> EmptyAction() => async () => { await Task.CompletedTask; };

        protected override void ExtendedUsePatterns()
        {
            Context["with actual async operations"] = () =>
            {
                It["should handle Task.Delay operations"] = () =>
                {
                    Check.ThatCode(() =>
                    {
                        Subject["should delay"] = async () =>
                        {
                            await Task.Delay(1);
                        };
                    }).DoesNotThrow();
                };

                It["should handle async method calls"] = () =>
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

        [Describe]
        public void AssertBuilderAsync()
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
