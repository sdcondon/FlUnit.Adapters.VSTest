using System;
using System.Collections.Generic;
using System.Threading;

namespace FlUnit.Adapters.VSTest
{
    /// <summary>
    /// The VSTest adapter's implementation of <see cref="ITestContext"/> - intended for tests to communicate with the test platform.
    /// </summary>
    internal class TestContext : ITestContext
    {
        private readonly Queue<string> outputMessages = new();
        private readonly Queue<string> errorMessages = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="TestContext"/> class.
        /// </summary>
        /// <param name="testCancellation">A cancellation token for test execution.</param>
        public TestContext(CancellationToken testCancellation)
        {
            TestCancellation = testCancellation;
        }

        /// <inheritdoc/>
        public CancellationToken TestCancellation { get; }

        /// <inheritdoc/>
        public void WriteOutput(string output)
        {
            outputMessages.Enqueue(output);
        }

        /// <inheritdoc/>
        public void WriteOutputLine(string output)
        {
            outputMessages.Enqueue(output + Environment.NewLine);
        }

        /// <inheritdoc/>
        public void WriteError(string error)
        {
            errorMessages.Enqueue(error);
        }

        /// <inheritdoc/>
        public void WriteErrorLine(string error)
        {
            errorMessages.Enqueue(error + Environment.NewLine);
        }

        /// <summary>
        /// Flushes all of the currently registered output messages. Intended to be called by the runner when recording a test result.
        /// NB: retrieval must be destructive because VSTest retrieves messages for each individual result. Yes, a cleaner approach
        /// might be to reset explicitly as each result is recorded, but this will do for now.
        /// </summary>
        /// <returns>An enumeration of the currently registered output messages.</returns>
        public IEnumerable<string> FlushOutputMessages()
        {
            while (outputMessages.Count > 0)
            {
                yield return outputMessages.Dequeue();
            }
        }

        /// <summary>
        /// Flushes all of the currently registered error messages. Intended to be called by the runner when recording a test result.
        /// NB: retrieval must be destructive because VSTest retrieves messages for each individual result. Yes, a cleaner approach
        /// might be to reset explicitly as each result is recorded, but this will do for now.
        /// </summary>
        /// <returns>An enumeration of the currently registered error messages.</returns>
        public IEnumerable<string> FlushErrorMessages()
        {
            while (errorMessages.Count > 0)
            {
                yield return errorMessages.Dequeue();
            }
        }
    }
}
