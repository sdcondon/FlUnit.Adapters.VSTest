using FlUnit.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

#if NET6_0_OR_GREATER
        public override ValueTask ArrangeAsync(ITestContext context)
        {
            cases = arrange(context);
            return ValueTask.CompletedTask;
        }
#else
        public override Task ArrangeAsync(ITestContext context)
        {
            cases = arrange(context);
            return Task.CompletedTask;
        }
#endif

        public override void Dispose()
        {
        }
    }
}
