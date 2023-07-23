namespace FlUnit.Adapters.VSTest
{
    /// <summary>
    /// Common constants, used across the VSTest adapter.
    /// </summary>
    internal static class Constants
    {
        /// <summary>
        /// Unique indentifying URI for the FlUnit test executor.
        /// </summary>
        internal const string ExecutorUri = "executor://sdcondon.net/FlUnit/v1";

        /// <summary>
        /// The name of the runsettings XML element that should contain FlUnit configuration settings.
        /// </summary>
        internal const string FlUnitConfigurationXmlElement = "FlUnit";
    }
}
