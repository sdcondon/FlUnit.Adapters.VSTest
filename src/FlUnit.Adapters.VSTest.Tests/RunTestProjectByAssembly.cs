using FluentAssertions;
using FlUnit.Adapters.VSTest._Tests.TestDoubles;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

namespace FlUnit.Adapters.VSTest._Tests
{
    using FlUnit.Adapters.VSTest.Tests.TestProject;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using System;

    [TestClass]
    public class RunTestProjectByAssembly
    {
        [TestMethod]
        public void Smoke()
        {
            var runner = new TestExecutor();
            var runContext = new Mock<IRunContext>();
            var frameworkHandle = new FakeFrameworkHandle();

            void AssertTestResult(string testName, IEnumerable<Trait> expectedTraits, TestOutcome expectedOutcome, IEnumerable<object> expectedResults)
            {
                frameworkHandle.TestCases.ContainsKey(testName).Should().BeTrue();
                frameworkHandle.TestCases[testName].Traits.Should().BeEquivalentTo(expectedTraits);
                frameworkHandle.TestOutcomes[testName].Should().Be(expectedOutcome);
                frameworkHandle.TestResults[testName].Should().BeEquivalentTo(expectedResults);
            }

            // Act
            runner.RunTests(
                sources: new[] { typeof(Tests).Assembly.Location },
                runContext.Object,
                frameworkHandle);

            // Assert
            AssertTestResult(
                "FlUnit.Adapters.VSTest.Tests.TestProject.Tests.ArrangementFailure",
                new Trait[]
                {
                    ////new Trait("ClassLevelTrait", "ArchetypalTests")
                },
                TestOutcome.Skipped,
                new[]
                {
                    // Lone results should have null display names, so as not to override the test name in the explorer..
                    new { DisplayName = (string)null, Outcome = TestOutcome.Skipped },
                });

            AssertTestResult(
                "FlUnit.Adapters.VSTest.Tests.TestProject.Tests.SingleCase_SingleAssertion_Positive",
                new Trait[]
                {
                    ////new Trait("ClassLevelTrait", "ArchetypalTests")
                },
                TestOutcome.Passed,
                new[]
                {
                    // Lone results should have null display names, so as not to override the test name in the explorer..
                    new { DisplayName = (string)null, Outcome = TestOutcome.Passed },
                });

            AssertTestResult(
                "FlUnit.Adapters.VSTest.Tests.TestProject.Tests.SingleCase_SingleAssertion_AssertionFailure",
                new Trait[]
                {
                    ////new Trait("ClassLevelTrait", "ArchetypalTests"),
                    ////new Trait("ExampleOfAFailingTest", null)
                },
                TestOutcome.Failed,
                new[]
                {
                    // Lone results should have null display names, so as not to override the test name in the explorer..
                    new { DisplayName = (string)null, Outcome = TestOutcome.Failed },
                });

            AssertTestResult(
                "FlUnit.Adapters.VSTest.Tests.TestProject.Tests.SingleCase_MultipleAssertions_Positive",
                new Trait[]
                {
                    ////new Trait("ClassLevelTrait", "ArchetypalTests"),
                    ////new Trait("ExampleOfAFailingTest", null)
                },
                TestOutcome.Passed,
                new[]
                {
                    new { DisplayName = "assertion 1", Outcome = TestOutcome.Passed },
                    new { DisplayName = "assertion 2", Outcome = TestOutcome.Passed },
                });

            AssertTestResult(
                "FlUnit.Adapters.VSTest.Tests.TestProject.Tests.SingleCase_MultipleAssertions_AssertionFailure",
                new Trait[]
                {
                    ////new Trait("ClassLevelTrait", "ArchetypalTests"),
                    ////new Trait("ExampleOfAFailingTest", null)
                },
                TestOutcome.Failed,
                new[]
                {
                    new { DisplayName = "assertion 1", Outcome = TestOutcome.Passed },
                    new { DisplayName = "assertion 2", Outcome = TestOutcome.Failed },
                });

            AssertTestResult(
                "FlUnit.Adapters.VSTest.Tests.TestProject.Tests.MultipleCases_MultipleAssertions_Positive",
                new Trait[]
                {
                    ////new Trait("ClassLevelTrait", "ArchetypalTests")
                },
                TestOutcome.Passed,
                new[]
                {
                    new { DisplayName = "For test case 1, assertion 1", Outcome = TestOutcome.Passed },
                    new { DisplayName = "For test case 1, assertion 2", Outcome = TestOutcome.Passed },
                    new { DisplayName = "For test case 2, assertion 1", Outcome = TestOutcome.Passed },
                    new { DisplayName = "For test case 2, assertion 2", Outcome = TestOutcome.Passed },
                });

            AssertTestResult(
                "FlUnit.Adapters.VSTest.Tests.TestProject.Tests.MultipleCases_MultipleAssertions_AssertionFailure",
                new Trait[]
                {
                    ////new Trait("ClassLevelTrait", "ArchetypalTests")
                },
                TestOutcome.Failed,
                new[]
                {
                    new { DisplayName = "For test case 1, assertion 1", Outcome = TestOutcome.Passed },
                    new { DisplayName = "For test case 1, assertion 2", Outcome = TestOutcome.Failed },
                    new { DisplayName = "For test case 2, assertion 1", Outcome = TestOutcome.Passed },
                    new { DisplayName = "For test case 2, assertion 2", Outcome = TestOutcome.Failed },
                });

            AssertTestResult(
                "FlUnit.Adapters.VSTest.Tests.TestProject.Tests.AtLeast5Seconds",
                new Trait[]
                {
                    ////new Trait("ClassLevelTrait", "ArchetypalTests")
                },
                TestOutcome.Passed,
                new[]
                {
                    // Lone results should have null display names, so as not to override the test name in the explorer..
                    new { DisplayName = (string)null, Outcome = TestOutcome.Passed },
                });

            AssertTestResult(
                "FlUnit.Adapters.VSTest.Tests.TestProject.Tests.TestOutput",
                new Trait[]
                {
                    ////new Trait("ClassLevelTrait", "ArchetypalTests")
                },
                TestOutcome.Passed,
                new[]
                {
                    new
                    {
                        DisplayName = "case 1",
                        Outcome = TestOutcome.Passed,
                        Messages = new[]
                        {
                            new { Category = "StdOutMsgs", Text = "Hello case 1" + Environment.NewLine },
                            new { Category = "StdOutMsgs", Text = "Hello case 1 assertion" + Environment.NewLine },
                        }
                    },

                    new
                    {
                        DisplayName = "case 2",
                        Outcome = TestOutcome.Passed,
                        Messages = new[]
                        {
                            new { Category = "StdOutMsgs", Text = "Hello case 2" + Environment.NewLine },
                            new { Category = "StdOutMsgs", Text = "Hello case 2 assertion" + Environment.NewLine },
                        }
                    },
                });

            ////AssertTestResult(
            ////    "FlUnit.Adapters.VSTest.Tests.TestProject.Tests.ActionFailure",
            ////    new Trait[]
            ////    {
            ////        ////new Trait("ClassLevelTrait", "ArchetypalTests")
            ////    },
            ////    TestOutcome.Passed,
            ////    new[]
            ////    {
            ////        new { DisplayName = (string)null, Outcome = TestOutcome.Passed },
            ////    });
        }
    }
}
