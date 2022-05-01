using System;

namespace FlUnit.Adapters.VSTest.Tests.TestProject.TestDoubles
{
    internal class FakeAssertion : ITestAssertion
    {
        private readonly Action assert;
        private readonly string toStringValue;

        public FakeAssertion(Action assert, string toStringValue) => (this.assert, this.toStringValue) = (assert, toStringValue);

        public void Assert() => assert();

        public override string ToString() => toStringValue;

        public string ToString(string format, IFormatProvider formatProvider) => toStringValue;
    }
}
