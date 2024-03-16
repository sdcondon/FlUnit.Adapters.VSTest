using System;
using System.Threading.Tasks;

namespace FlUnit.Adapters.VSTest.Tests.TestProject.TestDoubles
{
    internal class FakeAssertion : ITestAssertion
    {
        private readonly Action assert;
        private readonly string toStringValue;

        public FakeAssertion(Action assert, string toStringValue) => (this.assert, this.toStringValue) = (assert, toStringValue);

#if NET6_0_OR_GREATER
        public ValueTask AssertAsync()
        {
            assert();
            return ValueTask.CompletedTask;
        }
#else
        public Task AssertAsync()
        {
            assert();
            return Task.CompletedTask;
        }
#endif

        public override string ToString() => toStringValue;

        public string ToString(string format, IFormatProvider formatProvider) => toStringValue;
    }
}
