using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System.Collections.Generic;
using System.Linq;

namespace FlUnit.Adapters.VSTest
{
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using System.IO;

    /// <summary>
    /// FlUnit's implementation of <see cref="ITestDiscoverer"/> - takes responsibility for discovering tests in a given assembly or assemblies.
    /// </summary>
    [FileExtension(".exe")]
    [FileExtension(".dll")]
    [DefaultExecutorUri(Constants.ExecutorUriString)]
    public class TestDiscoverer : ITestDiscoverer
    {
        /// <inheritdoc />
        public void DiscoverTests(
            IEnumerable<string> sources,
            IDiscoveryContext discoveryContext,
            IMessageLogger logger,
            ITestCaseDiscoverySink discoverySink)
        {
            var testRunConfiguration = TestRunConfiguration.ReadFromXml(discoveryContext.RunSettings?.SettingsXml, Constants.FlUnitConfigurationXmlElement);
            MakeTestCases(sources, logger, testRunConfiguration).ForEach(tc => discoverySink.SendTestCase(tc));
        }

        internal static List<TestCase> MakeTestCases(
            IEnumerable<string> sources,
            IMessageLogger logger,
            TestRunConfiguration testRunConfiguration)
        {
            return sources.SelectMany(s => MakeTestCases(s, logger, testRunConfiguration)).ToList();
        }

        private static List<TestCase> MakeTestCases(
            string source,
            IMessageLogger logger,
            TestRunConfiguration testRunConfiguration)
        {
            logger.SendMessage(TestMessageLevel.Informational, $"Test discovery started for {source}");
            var testMetadata = TestDiscovery.FindTests(source, testRunConfiguration);

            var testCases = new List<TestCase>();
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

                foreach (var testMetadatum in testMetadata)
                {
                    var navigationData = diaSession?.GetNavigationData(
                        testMetadatum.TestProperty.DeclaringType.FullName,
                        testMetadatum.TestProperty.GetGetMethod().Name);

                    var testCase = new TestCase()
                    {
                        FullyQualifiedName = $"{testMetadatum.TestProperty.DeclaringType.FullName}.{testMetadatum.TestProperty.Name}",
                        ExecutorUri = Constants.ExecutorUri,
                        Source = source,
                        CodeFilePath = navigationData?.FileName,
                        LineNumber = navigationData?.MinLineNumber ?? 0,
                    };

                    // need to pay more attention to how the serialization between discovery and execution works..
                    // ..e.g. does the serialised version stick around? Do I need to worry about versioning test cases and executor version?
                    testCase.SetPropertyValue(TestProperties.FlUnitTestProp, testMetadatum.InternalData);
                    testCase.Traits.AddRange(testMetadatum.Traits.Select(t => new Trait(t.Name, t.Value)));
                    testCases.Add(testCase);

                    ////logger.SendMessage(
                    ////    TestMessageLevel.Informational,
                    ////    $"Found test case {testCase.FullyQualifiedName}. Traits: {string.Join(", ", testCase.Traits.Select(t => $"{t.Name}={t.Value}"))}");
                }
            }
            finally
            {
                diaSession?.Dispose();
            }

            return testCases;
        }
    }
}
