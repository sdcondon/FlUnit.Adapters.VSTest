using FlUnit.Configuration;
using System;
using System.Collections.Generic;

namespace FlUnit.Adapters.VSTest.Tests.TestProject.TestDoubles
{
    internal class FakeTest : Test
    {
        private readonly Func<ITestContext, IReadOnlyCollection<FakeCase>> arrange;
        private IReadOnlyCollection<FakeCase> cases;

        public FakeTest(Func<ITestContext, IReadOnlyCollection<FakeCase>> arrange)
        {
            this.arrange = arrange;
        }

        public override IReadOnlyCollection<ITestCase> Cases => cases;

        public override bool HasConfigurationOverrides => false;

        public override void ApplyConfigurationOverrides(ITestConfiguration testConfiguration)
        {
        }

        public override void Arrange(ITestContext context) => cases = arrange(context);

        public override void Dispose()
        {
        }
    }
}
