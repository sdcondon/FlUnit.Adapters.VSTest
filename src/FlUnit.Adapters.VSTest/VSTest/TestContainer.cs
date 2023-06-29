using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using System;
using System.Linq;

namespace FlUnit.Adapters.VSTest
{
    /// <summary>
    /// The VSTest adapter's implementation of <see cref="ITestContainer"/>.
    /// Intended for consumption (via its interface) by FlUnit's core execution logic (i.e. the <see cref="TestRun"/> class).
    /// </summary>
    internal class TestContainer : ITestContainer
    {
        private readonly TestCase testCase;
        private readonly IRunContext runContext;
        private readonly IFrameworkHandle frameworkHandle;
        private readonly TestContext testContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestContainer"/> class.
        /// </summary>
        /// <param name="testCase">The VSTest platform information for the test</param>
        /// <param name="runContext">The VSTest <see cref="IRunContext"/> that the test is being executed in.</param>
        /// <param name="frameworkHandle">The VSTest <see cref="IFrameworkHandle"/> that the test should use for callbacks.</param>
        public TestContainer(TestCase testCase, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            this.testCase = testCase;

            TestMetadata = new TestMetadata(
                (string)testCase.GetPropertyValue(TestProperties.FlUnitTestProp),
                testCase.Traits.Select(t => new Trait(t.Name, t.Value)));

            this.runContext = runContext;
            this.frameworkHandle = frameworkHandle;
            this.testContext = new TestContext();

            ////Dump();
        }

        /// <inheritdoc/>
        public TestMetadata TestMetadata { get; }

        /// <inheritdoc/>
        public ITestContext TestContext => testContext;

        /// <inheritdoc/>
        public void RecordStart()
        {
            frameworkHandle.RecordStart(testCase);
        }

        /// <inheritdoc/>
        public void RecordResult(
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            string displayName,
            TestOutcome outcome,
            string errorMessage,
            string errorStackTrace)
        {
            var result = new TestResult(testCase)
            {
                StartTime = startTime,
                EndTime = endTime,
                Duration = endTime - startTime,
                DisplayName = displayName,
                Outcome = MapOutcome(outcome),
                ErrorMessage = errorMessage,
                ErrorStackTrace = errorStackTrace,

            };

            foreach (var message in testContext.FlushOutputMessages())
            {
                result.Messages.Add(new TestResultMessage(TestResultMessage.StandardOutCategory, message));
            }

            foreach (var message in testContext.FlushErrorMessages())
            {
                result.Messages.Add(new TestResultMessage(TestResultMessage.StandardErrorCategory, message));
            }

            frameworkHandle.RecordResult(result);
        }

        /// <inheritdoc/>
        public void RecordEnd(TestOutcome outcome)
        {
            frameworkHandle.RecordEnd(testCase, MapOutcome(outcome));
        }

        private static Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome MapOutcome(TestOutcome flUnitOutome)
        {
            switch (flUnitOutome)
            {
                case TestOutcome.Passed:
                    return Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome.Passed;
                case TestOutcome.Failed:
                    return Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome.Failed;
                case TestOutcome.ArrangementFailed:
                    return Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome.Skipped;
                default:
                    throw new ArgumentException();
            };
        }

        /// <summary>
        /// Dumps all VSTest properties and traits to the framework as informational messages. 
        /// </summary>
        ////private void Dump()
        ////{
        ////    frameworkHandle.SendMessage(TestMessageLevel.Informational, $"DUMPING TEST '{testCase.GetPropertyValue(testCase.Properties.SingleOrDefault(p => p.Id == "TestCase.DisplayName"))}'");

        ////    foreach (var property in testCase.Properties)
        ////    {
        ////        frameworkHandle.SendMessage(TestMessageLevel.Informational, $"\tPROPERTY: {property.Id} - {testCase.GetPropertyValue(property)}");
        ////    }

        ////    foreach (var trait in testCase.Traits)
        ////    {
        ////        frameworkHandle.SendMessage(TestMessageLevel.Informational, $"\tTRAIT: {trait.Name} - {trait.Value}");
        ////    }
        ////}
    }
}
