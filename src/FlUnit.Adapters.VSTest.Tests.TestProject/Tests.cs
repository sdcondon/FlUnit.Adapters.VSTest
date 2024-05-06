using FlUnit.Adapters.VSTest.Tests.TestProject.TestDoubles;
using System;
using System.Threading.Tasks;

namespace FlUnit.Adapters.VSTest.Tests.TestProject
{
    ////[FakeTrait("ClassLevelTrait", nameof(ArchetypalTests))]
    public static class Tests
    {
        ////[FakeTrait("PropertyLevelTrait")]
        public static Test ArrangementFailure => new FakeTest(ctx =>
        {
            throw new InvalidOperationException("KABOOM");
        });

        public static Test SingleCase_SingleAssertion_Positive => new FakeTest(ctx => new FakeCase[]
        {
            new(
                act: () => { },
                assertions: new FakeAssertion[]
                {
                    new(() => { }, "lone assertion"),
                },
                toStringValue: "lone test case"),
        });

        public static Test SingleCase_SingleAssertion_AssertionFailure => new FakeTest(ctx => new FakeCase[]
        {
            new(
                act: () => { },
                assertions: new FakeAssertion[]
                {
                    new(() => throw new InvalidOperationException("KABOOM"), "lone assertion"),
                },
                toStringValue: "lone test case"),
        });

        public static Test SingleCase_MultipleAssertions_Positive => new FakeTest(ctx => new FakeCase[]
        {
            new(
                act: () => { },
                assertions: new FakeAssertion[]
                {
                    new(() => { }, "assertion 1"),
                    new(() => { }, "assertion 2"),
                },
                toStringValue: "lone test case"),
        });

        public static Test SingleCase_MultipleAssertions_AssertionFailure => new FakeTest(ctx => new FakeCase[]
        {
            new(
                act: () => { },
                assertions: new FakeAssertion[]
                {
                    new(() => { }, "assertion 1"),
                    new(() => throw new InvalidOperationException("KABOOM"), "assertion 2"),
                },
                toStringValue: "lone test case"),
        });

        public static Test MultipleCases_MultipleAssertions_Positive => new FakeTest(ctx => new FakeCase[]
        {
            new(
                act: () => { },
                assertions: new FakeAssertion[]
                {
                    new(() => { }, "assertion 1"),
                    new(() => { }, "assertion 2"),
                },
                toStringValue: "test case 1"),
            new(
                act: () => { },
                assertions: new FakeAssertion[]
                {
                    new(() => { }, "assertion 1"),
                    new(() => { }, "assertion 2"),
                },
                toStringValue: "test case 2"),
        });

        public static Test MultipleCases_MultipleAssertions_AssertionFailure => new FakeTest(ctx => new FakeCase[]
        {
            new(
                act: () => { },
                assertions: new FakeAssertion[]
                {
                    new(() => { }, "assertion 1"),
                    new(() => throw new InvalidOperationException("KABOOM"), "assertion 2"),
                },
                toStringValue: "test case 1"),
            new(
                act: () => { },
                assertions: new FakeAssertion[]
                {
                    new(() => { }, "assertion 1"),
                    new(() => throw new InvalidOperationException("KABOOM"), "assertion 2"),
                },
                toStringValue: "test case 2"),
        });

        public static Test AtLeast5Seconds => new FakeTest(ctx => new FakeCase[]
        {
            new(
                act: () => Task.Delay(TimeSpan.FromSeconds(5), ctx.TestCancellation).Wait(),
                assertions: new FakeAssertion[]
                {
                    new(() => { }, "lone assertion"),
                },
                toStringValue: "lone test case"),
        });

        public static Test TestOutput => new FakeTest(ctx => new FakeCase[]
        {
            new(
                act: () => ctx.WriteOutputLine("Hello case 1"),
                assertions: new FakeAssertion[]
                {
                    new(() => ctx.WriteOutputLine("Hello case 1 assertion"), "lone assertion"),
                },
                toStringValue: "case 1"),
            new(
                act: () => ctx.WriteOutputLine("Hello case 2"),
                assertions: new FakeAssertion[]
                {
                    new(() => ctx.WriteOutputLine("Hello case 2 assertion"), "lone assertion"),
                },
                toStringValue: "case 2"),
        });

        // TODO: an exception thrown by the test action is a bad test behaviour - tests are be expected
        // to capture any exception thrown by the tested code, so that it can be asserted on.
        // We should probably do something to handle it though, rather than blowing up completely.
        ////public static Test ActionFailure => new FakeTest(ctx => new FakeCase[]
        ////{
        ////    new(
        ////        act: () => throw new InvalidOperationException("KABOOM"),
        ////        assertions: new FakeAssertion[]
        ////        {
        ////            new(() => { }, "lone assertion"),
        ////        },
        ////        toStringValue: "lone test case"),
        ////});
    }
}
 