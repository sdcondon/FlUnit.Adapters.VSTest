using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System.Collections.Generic;
using System.Linq;

namespace FlUnit.Adapters.VSTest
{
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    /// <summary>
    /// FlUnit's implementation of <see cref="ITestDiscoverer"/> - takes responsibility for discovering tests in a given assembly or assemblies.
    /// </summary>
    [FileExtension(".exe")]
    [FileExtension(".dll")]
    [DefaultExecutorUri(Constants.ExecutorUri)]
    public class TestDiscoverer : ITestDiscoverer
    {
        /// <inheritdoc />
        public void DiscoverTests(
            IEnumerable<string> sources,
            IDiscoveryContext discoveryContext,
            IMessageLogger logger,
            ITestCaseDiscoverySink discoverySink)
        {
            try
            {
                foreach (var tc in MakeTestCases(sources, logger))
                {
                    discoverySink.SendTestCase(tc);
                }
            }
            catch (Exception e)
            {
                logger.SendMessage(TestMessageLevel.Error, $"FlUnit test discovery failure for [{string.Join(", ", sources)}]: {e}");
                throw;
            }
        }

        internal static IEnumerable<TestCase> MakeTestCases(
            IEnumerable<string> sources,
            IMessageLogger logger)
        {
            return sources.SelectMany(s => MakeTestCases(s, logger));
        }

        private static IEnumerable<TestCase> MakeTestCases(
            string source,
            IMessageLogger logger)
        {
            logger.SendMessage(TestMessageLevel.Informational, $"FlUnit test discovery started for {source} on {RuntimeInformation.FrameworkDescription} {RuntimeInformation.ProcessArchitecture}");

            DiaSession diaSession = null;
            try
            {
                try
                {
                    diaSession = new DiaSession(source);
                }
                catch (FileNotFoundException)
                {
                }

                foreach (var testMetadatum in TestDiscovery.FindTests(source))
                {
                    yield return TestContainer.MakeTestCase(testMetadatum, diaSession, source);
                }
            }
            finally
            {
                diaSession?.Dispose();
            }

            logger.SendMessage(TestMessageLevel.Informational, $"FlUnit test discovery completed for {source} on {RuntimeInformation.FrameworkDescription} {RuntimeInformation.ProcessArchitecture}");
        }
    }
}
