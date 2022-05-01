using FlUnit.Adapters.VSTest.Tests.TestProject.TestDoubles;
using System;
using System.Threading.Tasks;

namespace FlUnit.Adapters.VSTest.Tests.TestProject
{
    ////[Trait("ClassLevelTrait", nameof(ArchetypalTests))]
    public static class Tests
    {
        ////[Trait("PropertyLevelTrait")]
        public static Test ArrangementFailure => new FakeTest(ctx =>
        {
            throw new InvalidOperationException("KABOOM");
        });

        public static Test SingleCase_SingleAssertion_Positive => new FakeTest(ctx => new FakeCase[]
        {
            new FakeCase(
                act: () => { },
                assertions: new FakeAssertion[]
                {
                    new FakeAssertion(() => { }, "lone assertion"),
                },
                toStringValue: "lone test case"),
        });

        public static Test SingleCase_SingleAssertion_AssertionFailure => new FakeTest(ctx => new FakeCase[]
        {
            new FakeCase(
                act: () => { },
                assertions: new FakeAssertion[]
                {
                    new FakeAssertion(() => throw new InvalidOperationException("KABOOM"), "lone assertion"),
                },
                toStringValue: "lone test case"),
        });

        public static Test SingleCase_MultipleAssertions_Positive => new FakeTest(ctx => new FakeCase[]
        {
            new FakeCase(
                act: () => { },
                assertions: new FakeAssertion[]
                {
                    new FakeAssertion(() => { }, "assertion 1"),
                    new FakeAssertion(() => { }, "assertion 2"),
                },
                toStringValue: "lone test case"),
        });

        public static Test SingleCase_MultipleAssertions_AssertionFailure => new FakeTest(ctx => new FakeCase[]
        {
            new FakeCase(
                act: () => { },
                assertions: new FakeAssertion[]
                {
                    new FakeAssertion(() => { }, "assertion 1"),
                    new FakeAssertion(() => throw new InvalidOperationException("KABOOM"), "assertion 2"),
                },
                toStringValue: "lone test case"),
        });

        public static Test MultipleCases_MultipleAssertions_Positive => new FakeTest(ctx => new FakeCase[]
        {
            new FakeCase(
                act: () => { },
                assertions: new FakeAssertion[]
                {
                    new FakeAssertion(() => { }, "assertion 1"),
                    new FakeAssertion(() => { }, "assertion 2"),
                },
                toStringValue: "test case 1"),
            new FakeCase(
                act: () => { },
                assertions: new FakeAssertion[]
                {
                    new FakeAssertion(() => { }, "assertion 1"),
                    new FakeAssertion(() => { }, "assertion 2"),
                },
                toStringValue: "test case 2"),
        });

        public static Test MultipleCases_MultipleAssertions_AssertionFailure => new FakeTest(ctx => new FakeCase[]
        {
            new FakeCase(
                act: () => { },
                assertions: new FakeAssertion[]
                {
                    new FakeAssertion(() => { }, "assertion 1"),
                    new FakeAssertion(() => throw new InvalidOperationException("KABOOM"), "assertion 2"),
                },
                toStringValue: "test case 1"),
            new FakeCase(
                act: () => { },
                assertions: new FakeAssertion[]
                {
                    new FakeAssertion(() => { }, "assertion 1"),
                    new FakeAssertion(() => throw new InvalidOperationException("KABOOM"), "assertion 2"),
                },
                toStringValue: "test case 2"),
        });

        public static Test AtLeast2Seconds => new FakeTest(ctx => new FakeCase[]
        {
            new FakeCase(
                act: () => Task.Delay(TimeSpan.FromSeconds(2)).Wait(),
                assertions: new FakeAssertion[]
                {
                    new FakeAssertion(() => { }, "lone assertion"),
                },
                toStringValue: "lone test case"),
        });

        public static Test TestOutput => new FakeTest(ctx => new FakeCase[]
        {
            new FakeCase(
                act: () => ctx.WriteOutputLine("Hello world"),
                assertions: new FakeAssertion[]
                {
                    new FakeAssertion(() => ctx.WriteOutputLine("Hello again"), "lone assertion"),
                },
                toStringValue: "lone test case"),
        });

        // NB: an exception thrown by the test action is a bad test behaviour.
        // Ordinarily the test would be expected to capture any exception thrown by the tested code,
        // so that it can be asserted on.
        ////public static Test ActionFailure => new FakeTest(ctx => new FakeCase[]
        ////{
        ////    new FakeCase(
        ////        act: () => throw new InvalidOperationException("KABOOM"),
        ////        assertions: new FakeAssertion[]
        ////        {
        ////            new FakeAssertion(() => { }, "lone assertion"),
        ////        },
        ////        toStringValue: "lone test case"),
        ////});
    }
}
 