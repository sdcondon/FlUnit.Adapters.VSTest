using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FlUnit.Adapters.VSTest.Tests
{
    [TestClass]
    public class TestContainerPartitionerTests
    {
        [TestMethod]
        public void Smoke()
        {
            var partitioner = new TestContainerTraitPartitioner(new FakeTestContainer[]
            {
                new FakeTestContainer("1"),
                new FakeTestContainer("2"),
                new FakeTestContainer("3"),
                new FakeTestContainer("4"),

                new FakeTestContainer("1"),
                new FakeTestContainer("2"),
                new FakeTestContainer("3"),

                new FakeTestContainer("1"),
                new FakeTestContainer("2"),

                new FakeTestContainer("1"),
            }, 
            "MyTrait");

            var dynamicPartitions = partitioner.GetDynamicPartitions();
            var partition1 = dynamicPartitions.GetEnumerator();
            NextValueShouldExistAndHaveTraitValue(partition1, "1");
            var partition2 = dynamicPartitions.GetEnumerator();
            NextValueShouldExistAndHaveTraitValue(partition2, "2");
            NextValueShouldExistAndHaveTraitValue(partition1, "1");
            var partition3 = dynamicPartitions.GetEnumerator();
            NextValueShouldExistAndHaveTraitValue(partition3, "3");
            NextValueShouldExistAndHaveTraitValue(partition2, "2");
            NextValueShouldExistAndHaveTraitValue(partition1, "1");
            var partition4 = dynamicPartitions.GetEnumerator();
            NextValueShouldExistAndHaveTraitValue(partition4, "4");
            NextValueShouldExistAndHaveTraitValue(partition3, "3");
            NextValueShouldExistAndHaveTraitValue(partition2, "2");
            NextValueShouldExistAndHaveTraitValue(partition1, "1");
            var partition5 = dynamicPartitions.GetEnumerator();
            partition5.MoveNext().Should().BeFalse();
            partition4.MoveNext().Should().BeFalse();
            partition3.MoveNext().Should().BeFalse();
            partition2.MoveNext().Should().BeFalse();
            partition1.MoveNext().Should().BeFalse();
        }

        private static void NextValueShouldExistAndHaveTraitValue(IEnumerator<ITestContainer> partition, string value)
        {
            partition.MoveNext().Should().BeTrue();
            partition.Current.TestMetadata.Traits.Single(t => t.Name == "MyTrait").Value.Should().Be(value);
        }

        private class FakeTestContainer : ITestContainer
        {
            public FakeTestContainer(string traitValue)
            {
                TestMetadata = new TestMetadata((PropertyInfo)null, new[]
                {
                    new TraitAttribute("MyTrait", traitValue)
                });
            }

            public TestMetadata TestMetadata { get; }

            public ITestContext TestContext => throw new NotImplementedException();

            public void RecordEnd(TestOutcome outcome) => 
                throw new NotImplementedException();

            public void RecordResult(DateTimeOffset startTime, DateTimeOffset endTime, string displayName, TestOutcome outcome, string errorMessage, string errorStackTrace) =>
                throw new NotImplementedException();

            public void RecordStart() =>
                throw new NotImplementedException();
        }
    }
}
