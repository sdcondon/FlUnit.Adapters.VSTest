using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FlUnit.Adapters
{
    /// <summary>
    /// Core logic for finding FlUnit tests.
    /// </summary>
    internal static class TestDiscovery
    {
        /// <summary>
        /// Finds all of the properties that represent tests in a given assembly - along with the traits that are associated with each.
        /// </summary>
        /// <param name="assemblyPath">The path of the assembly file to examine for tests.</param>
        /// <param name="runConfiguration">Unused for the moment. Sue me.</param>
        /// <returns>An enumerable of <see cref="TestMetadata"/>, one for each discovered test.</returns>
        public static IEnumerable<TestMetadata> FindTests(string assemblyPath, TestRunConfiguration runConfiguration)
        {
            // TODO-BUG: discovery for assemblies that e.g. target a different platform than that of the discovering app
            // is going to fail with a BadImageFormatException at this point. Other frameworks tend to use reflection-only load
            // for discovery. Of course, we want to allow test code execution on discovery to allow for platform test
            // granularities other than PerTest. At some point, should add graceful fallback to reflection-only load - perhaps
            // with a logged warning that any config settings that are relevant at test discovery time (i.e. granularity)
            // will not be applied.
            var assembly = Assembly.LoadFile(assemblyPath);

            var assemblyTraitProviders = assembly.GetCustomAttributes().OfType<ITraitProvider>();

            // NB: Possible performance concerns here. Benchmarks proj shows that an AsParallel
            // here slows example test proj run down, though. That may just be due to its small
            // size, but then most test projects could probably be expected to be small?
            // More testing needed before doing anything differently here.
            return assembly.ExportedTypes
                .Select(t => ConcatTraitProviders(t, assemblyTraitProviders))
                .SelectMany(t => t.member.GetProperties().Where(IsTestProperty).Select(p =>
                {
                    var (testProperty, traitProviders) = ConcatTraitProviders(p, t.traitProviders);
                    return new TestMetadata(testProperty, traitProviders.Select(tp => tp.GetTrait(testProperty)));
                }));
        }

        private static (T member, IEnumerable<ITraitProvider> traitProviders) ConcatTraitProviders<T>(T memberInfo, IEnumerable<ITraitProvider> traitProviders)
            where T : MemberInfo
        {
            return (memberInfo, traitProviders: traitProviders.Concat(memberInfo.GetCustomAttributes().OfType<ITraitProvider>()));
        }

        private static bool IsTestProperty(PropertyInfo p)
        {
            return p.CanRead
                && p.GetMethod.IsPublic
                && p.GetMethod.IsStatic
                && typeof(Test).IsAssignableFrom(p.PropertyType);
        }
    }
}
