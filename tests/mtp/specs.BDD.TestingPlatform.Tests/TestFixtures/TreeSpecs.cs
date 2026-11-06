using specs.BDD;
using specs.BDD.Attributes;

namespace specs.BDD.TestingPlatform.Tests.TestFixtures
{
    // These classes are discovered by SpecTreeBuilder in tests; they are never run by NUnit.

    public class TreeSpec : Specifications
    {
        [Feature("Cart")]
        public void describe_cart()
        {
            It["adds an item"] = () => { };
            It["removes an item"] = () => { };

            Context["when empty"] = () =>
            {
                It["shows a message"] = () => { };
            };
        }

        [Feature]
        public void describe_checkout()
        {
            It["charges the card"] = () => { };
        }
    }

    public class GroupedTreeSpec : Specifications
    {
        [Feature("grouped", Level = ReportingLevel.Feature)]
        public void describe_grouped()
        {
            It["first"] = () => { };

            Context["nested"] = () =>
            {
                It["second"] = () => { };
            };
        }

        [Feature("all pending", Level = ReportingLevel.Feature)]
        public void describe_all_pending()
        {
            It["later"] = TODO;
        }
    }

    public class PendingTreeSpec : Specifications
    {
        [Feature("pending")]
        public void describe_pending()
        {
            It["runs"] = () => { };
            It["waits"] = TODO;
        }
    }

    public class SlashTreeSpec : Specifications
    {
        [Feature("input/output")]
        public void describe_io()
        {
            It["100% done"] = () => { };
        }
    }

    public class NoDescribeSpec : Specifications
    {
    }

    public class OuterSpec
    {
        public class InnerSpec : Specifications
        {
            [Feature("inner")]
            public void describe_inner()
            {
                It["works"] = () => { };
            }
        }
    }
}
