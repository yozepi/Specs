using specs.core.Framework.Adapters;
using System;
using System.Collections.Generic;

namespace specs.core.Framework.Processors.TestCases
{
    internal interface  ITestCase
    {
        Action SetupOnce { get; }
        Action SetupEach { get; }
        Action Act { get; }
        Action TeardownEach { get; }
        Action TeardownOnce { get; }
        IReadOnlyList<(string description, Action test)> Tests { get; }
    }
}
