using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlUnit.Adapters.VSTest.Tests.TestProject.TestDoubles
{
    internal class FakeCase : ITestCase
    {
        private readonly Action act;
        private readonly string toStringValue;

        public FakeCase(Action act, IReadOnlyCollection<FakeAssertion> assertions, string toStringValue)
        {
            this.act = act;
            Assertions = assertions;
            this.toStringValue = toStringValue;
        }

        public IReadOnlyCollection<ITestAssertion> Assertions { get; }

#if NET6_0_OR_GREATER
        public ValueTask ActAsync()
        {
            act();
            return ValueTask.CompletedTask;
        }
#else
        public Task ActAsync()
        {
            act();
            return Task.CompletedTask;
        }
#endif

        public override string ToString() => toStringValue;

        public string ToString(string format, IFormatProvider formatProvider) => toStringValue;
    }
}
