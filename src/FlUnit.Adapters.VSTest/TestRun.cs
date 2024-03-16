using FlUnit.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FlUnit.Adapters
{
    /// <summary>
    /// FlUnit's core test execution logic. Instances of this class encapsulate a FlUnit test run.
    /// </summary>
    internal class TestRun
    {
        private readonly IEnumerable<ITestContainer> testContainers;
        private readonly TestRunConfiguration testRunConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestRun"/> class.
        /// </summary>
        /// <param name="testContainers">An enumerable of <see cref="ITestContainer"/> instances, one for each of the tests to be run.</param>
        /// <param name="testRunConfiguration">The configuration that applies to this test run.</param>
        public TestRun(IEnumerable<ITestContainer> testContainers, TestRunConfiguration testRunConfiguration)
        {
            this.testContainers = testContainers;
            this.testRunConfiguration = testRunConfiguration;
        }

        /// <summary>
        /// Executes the test run.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token that, if triggered, should cause the test run to abort.</param>
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (testRunConfiguration.Parallelise)
            {
                if (!string.IsNullOrEmpty(testRunConfiguration.ParallelPartitioningTrait))
                {
#if NET6_0_OR_GREATER
                    Parallel.ForEach(
                        new TestContainerTraitPartitioner(testContainers, testRunConfiguration.ParallelPartitioningTrait),
                        new ParallelOptions()
                        {
                            CancellationToken = cancellationToken
                        },
                        tc => RunTestAsync(tc, testRunConfiguration.TestConfiguration).AsTask().GetAwaiter().GetResult());
#else
                    Parallel.ForEach(
                        new TestContainerTraitPartitioner(testContainers, testRunConfiguration.ParallelPartitioningTrait),
                        new ParallelOptions()
                        {
                            CancellationToken = cancellationToken
                        },
                        tc => RunTestAsync(tc, testRunConfiguration.TestConfiguration).GetAwaiter().GetResult());
#endif
                }
                else
                {
#if NET6_0_OR_GREATER
                    await Parallel.ForEachAsync(
                        testContainers,
                        new ParallelOptions()
                        {
                            CancellationToken = cancellationToken
                        },
                        (tc, ct) => RunTestAsync(tc, testRunConfiguration.TestConfiguration));
#else
                    Parallel.ForEach(
                        testContainers,
                        new ParallelOptions()
                        {
                            CancellationToken = cancellationToken
                        },
                        tc => RunTestAsync(tc, testRunConfiguration.TestConfiguration).GetAwaiter().GetResult());
#endif
                }
            }
            else
            {
                foreach (var testContainer in testContainers)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await RunTestAsync(testContainer, testRunConfiguration.TestConfiguration);
                }
            }
        }

#if NET6_0_OR_GREATER
        private async ValueTask RunTestAsync(ITestContainer testContainer, TestConfiguration testConfiguration)
#else
        private async Task RunTestAsync(ITestContainer testContainer, TestConfiguration testConfiguration)
#endif
        {
            using (var test = (Test)testContainer.TestMetadata.TestProperty.GetValue(null))
            {
                if (test.HasConfigurationOverrides)
                {
                    testConfiguration = testConfiguration.Clone();
                    test.ApplyConfigurationOverrides(testConfiguration);
                }

                testContainer.RecordStart();

                var testArrangementPassed = await TryArrangeTestInstanceAsync(test, testContainer, testConfiguration);

                var allAssertionsPassed = testArrangementPassed;
                if (testArrangementPassed)
                {
                    foreach (var testCase in test.Cases)
                    {
                        var startTime = DateTimeOffset.Now;
                        await testCase.ActAsync();

                        foreach (var assertion in testCase.Assertions)
                        {
                            allAssertionsPassed &= await CheckTestAssertionAsync(test, testCase, startTime, assertion, testConfiguration, testContainer);
                            startTime = DateTimeOffset.Now;
                        }
                    }
                }

                TestOutcome testOutcome;
                if (!testArrangementPassed)
                {
                    testOutcome = testConfiguration.ArrangementFailureCountsAsFailed ? TestOutcome.Failed : TestOutcome.ArrangementFailed;
                }
                else if (!allAssertionsPassed)
                {
                    testOutcome = TestOutcome.Failed;
                }
                else
                {
                    testOutcome = TestOutcome.Passed;
                }

                testContainer.RecordEnd(testOutcome);
            }
        }

        private static async Task<bool> TryArrangeTestInstanceAsync(Test test, ITestContainer testContainer, ITestConfiguration testConfiguration)
        {
            var arrangementStartTime = DateTimeOffset.Now;

            try
            {
                await test.ArrangeAsync(testContainer.TestContext);
                return true;
            }
            catch (Exception e)
            {
                var (errorMessage, errorStackTrace) = GetErrorDetails(e);

                testContainer.RecordResult(
                    startTime: arrangementStartTime,
                    endTime: DateTimeOffset.Now,
                    displayName: null,
                    outcome: testConfiguration.ArrangementFailureCountsAsFailed ? TestOutcome.Failed : TestOutcome.ArrangementFailed,
                    errorMessage: errorMessage,
                    errorStackTrace: errorStackTrace);

                return false;
            }
        }

#if NET6_0_OR_GREATER
        private static async ValueTask<bool> CheckTestAssertionAsync(Test test, ITestCase testCase, DateTimeOffset startTime, ITestAssertion assertion, ITestConfiguration testConfiguration, ITestContainer testContainer)
#else
        private static async Task<bool> CheckTestAssertionAsync(Test test, ITestCase testCase, DateTimeOffset startTime, ITestAssertion assertion, ITestConfiguration testConfiguration, ITestContainer testContainer)
#endif
        {
            // NB: Because VSTest test duration is always the sum of the test result durations, having results for each assertion
            // is fighting VSTest a little bit. Rather than include the test action duration in each test result (and this have
            // reported test duration multiply for every assertion that is added), we only include the action duration in the first
            // assertion for any given case (noting that using e.g. parameterless ThenReturns() will then give you something very
            // close to the action duration as the duration of the first result).
            // This is something to consider configurability for at some point - probably in the context of wider test execution strategy
            // (e.g. allowing for re-running the test action for each assertion)
            string displayName = null;
            TestOutcome outcome = TestOutcome.Failed;
            string errorMessage = null;
            string errorStackTrace = null;

            try
            {
                displayName = testConfiguration.ResultNamingStrategy.GetResultName(test, testCase, assertion);

                await assertion.AssertAsync();
                outcome = TestOutcome.Passed;
                return true;
            }
            catch (Exception e)
            {
                outcome = TestOutcome.Failed;
                (errorMessage, errorStackTrace) = GetErrorDetails(e);
                return false;
            }
            finally
            {
                testContainer.RecordResult(
                    startTime,
                    DateTimeOffset.Now,
                    displayName,
                    outcome,
                    errorMessage,
                    errorStackTrace);
            }
        }

        private static (string errorMessage, string errorStackTrace) GetErrorDetails(Exception exception)
        {
            if (exception is ITestFailureDetails tfd)
            {
                return (tfd.TestResultErrorMessage, tfd.TestResultErrorStackTrace);
            }
            else
            {
                return (exception.Message, exception.StackTrace);
            }
        }
    }
}
