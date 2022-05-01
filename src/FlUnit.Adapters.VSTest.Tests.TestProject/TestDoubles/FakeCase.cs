using System;
using System.Collections.Generic;

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

        public void Act() => act();

        public override string ToString() => toStringValue;

        public string ToString(string format, IFormatProvider formatProvider) => toStringValue;
    }
}
