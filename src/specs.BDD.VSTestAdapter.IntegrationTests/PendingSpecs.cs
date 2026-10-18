using specs.BDD;
using specs.BDD.Attributes;

namespace specs.BDD.VSTestAdapter.IntegrationTests;

/// <summary>
/// Integration specs for pending support — pending specs must be reported as Skipped and
/// never executed. Try <c>dotnet test --filter IsPending=true</c>.
/// </summary>
public class PendingSpecs : Specifications
{
    [Describe("pending specs")]
    public void describe_pending_specs()
    {
        It["runs a regular spec"] = () => { };

        It["reports a TODO spec as skipped"] = TODO;

        ItAsync["reports a TODOAsync spec as skipped"] = TODOAsync;

        xIt["never runs an excluded spec"] = () => throw new Exception("pending body must not run");

        xContext["a pending context"] = () =>
        {
            BeforeAll(() => throw new Exception("pending context BeforeAll must not run"));

            It["never runs specs inside it"] = () => throw new Exception("pending body must not run");
        };
    }

    [Describe("grouped pending specs", Reporting = SpecReporting.GroupedByRootContext)]
    public void describe_grouped_pending_specs()
    {
        It["runs a regular spec"] = () => { };

        It["lists a TODO spec as pending"] = TODO;

        xDescribe["a pending describe"] = () =>
        {
            It["lists nested specs as pending"] = () => throw new Exception("pending body must not run");
        };
    }
}
