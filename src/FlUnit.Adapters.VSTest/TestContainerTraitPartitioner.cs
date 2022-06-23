using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace FlUnit.Adapters
{
    /// <summary>
    /// Implementation of <see cref="Partitioner{TSource}"/> that partitions <see cref="ITestContainer"/> by the value of a given trait.
    /// That is, all tests with the same value of a given trait are guarenteed to occur in the same partition.
    /// </summary>
    internal class TestContainerTraitPartitioner : Partitioner<ITestContainer>
    {
        private readonly IEnumerable<IEnumerable<ITestContainer>> testsByTrait;
        private readonly object nextGroupLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="TestTraitPartitioner"/> class.
        /// </summary>
        /// <param name="traitName"></param>
        public TestContainerTraitPartitioner(IEnumerable<ITestContainer> source, string traitName)
            : base()
        {
            // I wonder if we could/should put null/none values into any partition - but consumers may assume otherwise..
            // This way (treating null/none as its own partition) is perhaps less load-balancy, but more intuitive.
            this.testsByTrait = source.GroupBy(c => c.TestMetadata.Traits.SingleOrDefault(t => t.Name == traitName)?.Value);
        }

        /// <inheritdoc/>
        public override bool SupportsDynamicPartitions => true;

        /// <inheritdoc/>
        public override IList<IEnumerator<ITestContainer>> GetPartitions(int partitionCount)
        {
            var dynamicPartitions = GetDynamicPartitions();

            var partitions = new IEnumerator<ITestContainer>[partitionCount];
            for (int i = 0; i < partitionCount; i++)
            {
                partitions[i] = dynamicPartitions.GetEnumerator();
            }

            return partitions;
        }

        /// <inheritdoc/>
        public override IEnumerable<ITestContainer> GetDynamicPartitions()
        {
            return new TestTraitDynamicPartitions(testsByTrait, nextGroupLock);
        }

        /// <remarks>
        /// MS' documentation on custom partitioners could be better - though the presence of a reference example is decisive.
        /// The idea seems to be for each call to <see cref="IEnumerable{T}.GetEnumerator"/> to return the next dynamic partition.
        /// </remarks>
        private class TestTraitDynamicPartitions : IEnumerable<ITestContainer>
        {
            private readonly IEnumerator<IEnumerable<ITestContainer>> groupEnumerator;
            private readonly object nextGroupLock;

            public TestTraitDynamicPartitions(IEnumerable<IEnumerable<ITestContainer>> testsByTrait, object nextGroupLock)
            {
                this.groupEnumerator = testsByTrait.GetEnumerator();
                this.nextGroupLock = nextGroupLock;
            }

            public IEnumerator<ITestContainer> GetEnumerator()
            {
                IEnumerator<ITestContainer> currentGroupEnumerator = null;

                while (true)
                {
                    if (currentGroupEnumerator == null || !currentGroupEnumerator.MoveNext())
                    {
                        lock (nextGroupLock)
                        {
                            if (!groupEnumerator.MoveNext())
                            {
                                yield break;
                            }

                            currentGroupEnumerator = groupEnumerator.Current.GetEnumerator();
                            continue;
                        }    
                    }

                    yield return currentGroupEnumerator.Current;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
