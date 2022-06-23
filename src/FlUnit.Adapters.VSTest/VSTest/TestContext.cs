using System;
using System.Collections.Generic;

namespace FlUnit.Adapters.VSTest
{
    /// <summary>
    /// The VSTest adapter's implementation of <see cref="ITestContext"/> - intended for tests to communicate with the test platform.
    /// </summary>
    internal class TestContext : ITestContext
    {
        private readonly Queue<string> outputMessages = new Queue<string>();
        private readonly Queue<string> errorMessages = new Queue<string>();

        /// <inheritdoc/>
        public void WriteOutput(string output)
        {
            outputMessages.Enqueue(output);
        }

        /// <inheritdoc/>
        public void WriteOutputLine(string output)
        {
            outputMessages.Enqueue(output);
            outputMessages.Enqueue(Environment.NewLine);
        }

        /// <inheritdoc/>
        public void WriteError(string error)
        {
            errorMessages.Enqueue(error);
        }

        /// <inheritdoc/>
        public void WriteErrorLine(string error)
        {
            errorMessages.Enqueue(error);
            errorMessages.Enqueue(Environment.NewLine);
        }

        /// <summary>
        /// Flushes all of the currently registered output messages. Intended to be called by the runner when recording a test result.
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
