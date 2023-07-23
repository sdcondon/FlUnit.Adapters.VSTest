using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using System;
using System.Linq;
using VSTestTrait = Microsoft.VisualStudio.TestPlatform.ObjectModel.Trait;
using FlUnitTrait = FlUnit.Adapters.Trait;

namespace FlUnit.Adapters.VSTest
{
    /// <summary>
    /// The VSTest adapter's implementation of <see cref="ITestContainer"/>.
    /// Intended for consumption (via its interface) by FlUnit's core execution logic (i.e. the <see cref="TestRun"/> class).
    /// </summary>
    internal class TestContainer : ITestContainer
    {
        // NB: for some unfathomable reason, property IDs have to be Pascal-cased, with the framework raising an unguessable error message for camel-casing.
        // But not until after the property has been registered. A violated assumption during the serialization process, maybe?
        private static readonly TestProperty FlUnitTestProp = TestProperty.Register(
            "FlUnitTestCase",
            "flUnit Test Case",
            typeof(string),
            typeof(TestExecutor));

        private readonly TestCase testCase;
        private readonly IRunContext runContext;
        private readonly IFrameworkHandle frameworkHandle;
        private readonly TestContext testContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestContainer"/> class.
        /// </summary>
        /// <param name="testCase">The VSTest platform information for the test</param>
        /// <param name="testMetadata">The FlUnit information for the test.</param>
        /// <param name="runContext">The VSTest <see cref="IRunContext"/> that the test is being executed in.</param>
        /// <param name="frameworkHandle">The VSTest <see cref="IFrameworkHandle"/> that the test should use for callbacks.</param>
        public TestContainer(TestCase testCase, TestMetadata testMetadata, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            this.testCase = testCase;
            this.TestMetadata = testMetadata;
            this.runContext = runContext;
            this.frameworkHandle = frameworkHandle;
            this.testContext = new TestContext();

            ////Dump();
        }

        /// <inheritdoc/>
        public TestMetadata TestMetadata { get; }

        /// <summary>
        /// Instantiates a VSTest <see cref="TestCase"/> from which a <see cref="TestContainer"/> can be loaded (with <see cref="Load"/> - generally after a serialization roundtrip).
        /// </summary>
        /// <param name="testMetadata">The FlUnit data for the test.</param>
        /// <param name="diaSession">The diagnostics session from which to retrieve source information. Can be null.</param>
        /// <param name="source">The file path of test assembly.</param>
        /// <returns>A new <see cref="TestCase"/> instance.</returns>
        public static TestCase MakeTestCase(TestMetadata testMetadata, DiaSession diaSession, string source)
        {
            var navigationData = diaSession?.GetNavigationData(
                testMetadata.TestProperty.DeclaringType.FullName,
                testMetadata.TestProperty.GetGetMethod().Name);

            var testCase = new TestCase()
            {
                FullyQualifiedName = $"{testMetadata.TestProperty.DeclaringType.FullName}.{testMetadata.TestProperty.Name}",
                ExecutorUri = new Uri(Constants.ExecutorUri),
                Source = source,
                CodeFilePath = navigationData?.FileName,
                LineNumber = navigationData?.MinLineNumber ?? 0,
            };

            // need to pay more attention to how the serialization between discovery and execution works..
            // ..e.g. does the serialised version stick around? Do I need to worry about versioning test cases and executor version?
            testCase.SetPropertyValue(FlUnitTestProp, testMetadata.InternalData);
            testCase.Traits.AddRange(testMetadata.Traits.Select(t => new VSTestTrait(t.Name, t.Value)));

            return testCase;
        }

        /// <summary>
        /// Loads a new instance of the <see cref="TestContainer"/> class from property information in a given <see cref="TestCase"/>.
        /// </summary>
        /// <param name="testCase">The VSTest platform information for the test</param>
        /// <param name="runContext">The VSTest <see cref="IRunContext"/> that the test is being executed in.</param>
        /// <param name="frameworkHandle">The VSTest <see cref="IFrameworkHandle"/> that the test should use for callbacks.</param>
        public static TestContainer Load(TestCase testCase, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            var testMetadata = TestMetadata.Load(
                (string)testCase.GetPropertyValue(FlUnitTestProp),
                testCase.Traits.Select(t => new FlUnitTrait(t.Name, t.Value)));

            return new TestContainer(testCase, testMetadata, runContext, frameworkHandle);
        }

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
