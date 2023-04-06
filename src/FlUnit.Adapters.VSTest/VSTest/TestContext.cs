using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FlUnit.Adapters.VSTest
{
    /// <summary>
    /// The VSTest adapter's implementation of <see cref="ITestContext"/> - intended for tests to communicate with the test platform.
    /// </summary>
    internal class TestContext : ITestContext
    {
        private readonly List<string> outputMessages = new List<string>();
        private readonly List<string> errorMessages = new List<string>();

        public IEnumerable<string> OutputMessages => outputMessages;

        public IEnumerable<string> ErrorMessages => errorMessages;

        /// <inheritdoc/>
        public void WriteOutput(string output)
        {
            outputMessages.Add(output);
        }

        /// <inheritdoc/>
        public void WriteOutputLine(string output)
        {
            outputMessages.Add(output);
            outputMessages.Add(Environment.NewLine);
        }

        /// <inheritdoc/>
        public void WriteError(string error)
        {
            errorMessages.Add(error);
        }

        /// <inheritdoc/>
        public void WriteErrorLine(string error)
        {
            errorMessages.Add(error);
            errorMessages.Add(Environment.NewLine);
        }
    }
}
