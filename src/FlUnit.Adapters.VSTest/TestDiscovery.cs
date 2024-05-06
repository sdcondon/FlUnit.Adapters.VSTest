using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace FlUnit.Adapters
{
    /// <summary>
    /// Core logic for finding FlUnit tests.
    /// </summary>
    internal static class TestDiscovery
    {
        private static readonly AssemblyName AbstractionsAssemblyName = new("FlUnit.Abstractions");

        /// <summary>
        /// Finds all of the properties that represent tests in a given assembly - along with the traits that are associated with each.
        /// </summary>
        /// <param name="testAssemblyPath">The path of the assembly file to examine for tests.</param>
        /// <returns>
        /// An enumerable of <see cref="TestMetadata"/>, one for each discovered test.
        /// NB: the returned metadata must be processed immediately, within the enumeration. Once the enumeration is complete,
        /// attempting to access them will result in an exception, since the load context will have been disposed.
        /// </returns>
        public static IEnumerable<TestMetadata> FindTests(string testAssemblyPath)
        {
            // NB: while the test assembly doesn't necessarily target the exact same framework
            // as the current one, the test platform does of course execute discovery using a compatible
            // framework, so can safely use current runtime dir here.
            var assemblyPaths = new List<string>();
            assemblyPaths.AddRange(Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll"));
            assemblyPaths.AddRange(Directory.GetFiles(Path.GetDirectoryName(testAssemblyPath), "*.dll"));
            var resolver = new PathAssemblyResolver(assemblyPaths);

            using var mlc = new MetadataLoadContext(resolver);
            var testAssembly = mlc.LoadFromAssemblyPath(testAssemblyPath);
            var flUnitAbstractionsAssembly = mlc.LoadFromAssemblyName(AbstractionsAssemblyName);

            foreach (var testMetadata in FindTests(testAssembly, flUnitAbstractionsAssembly))
            {
                yield return testMetadata;
            }
        }

        private static IEnumerable<TestMetadata> FindTests(Assembly testAssembly, Assembly flUnitAbstractionsAssembly)
        {
            var projectAbstractionsVersion = GetFlUnitAbstractionsReferenceMajorVersion(testAssembly);

            // NB: We could be passed an assembly that doesn't use FlUnit - this is fine and not an error - just means we won't find any tests.
            if (projectAbstractionsVersion == null)
            {
                return Enumerable.Empty<TestMetadata>();
            }

            // NB: We respect semantic versioning - major revisions mean breaking changes, so
            // in general we only support a single major version of the abstractions - the one referenced by this assembly.
            var supportedAbstractionsVersion = GetFlUnitAbstractionsReferenceMajorVersion(Assembly.GetExecutingAssembly());

            if (projectAbstractionsVersion > supportedAbstractionsVersion)
            {
                throw new ArgumentException($"The test project references a version of FlUnit that references a version of FlUnit.Abstractions (v{projectAbstractionsVersion}) that is more recent than the version that this version of the adapter supports (v{supportedAbstractionsVersion})." +
                    $"The test adapter needs to be upgraded to one that references v{supportedAbstractionsVersion}.x.x of the FlUnit.Abstractions package (or of course the version of FlUnit used could be downgraded).");
            }
            else if (projectAbstractionsVersion < supportedAbstractionsVersion)
            {
                throw new ArgumentException($"The test project uses a version of FlUnit that references a version of FlUnit.Abstractions (v{projectAbstractionsVersion}) that is older than the version that this version of the adapter supports (v{supportedAbstractionsVersion})." +
                    $"The version of FlUnit used needs to be upgraded to one that references v{supportedAbstractionsVersion}.x.x of the FlUnit.Abstractions package (or of course the version of the adapter used could be downgraded).");
            }

            // NB: Can't mix and match loaded and MLC types - so have to do this.
            var testType = flUnitAbstractionsAssembly.GetType(typeof(Test).FullName);
            var traitType = flUnitAbstractionsAssembly.GetType(typeof(TraitAttribute).FullName);

            var assemblyTraits = GetTraits(testAssembly.GetCustomAttributesData(), traitType);

            // NB: Possible performance concerns here. Benchmarks proj shows that an AsParallel
            // here slows example test proj run down, though. That may just be due to its small
            // size, but then most test projects could probably be expected to be small?
            // More testing needed before doing anything differently here.
            return testAssembly
                .ExportedTypes.Select(typeInfo => ConcatTraits(typeInfo, assemblyTraits, traitType))
                .SelectMany(t => t.memberInfo.GetProperties().Where(p => IsTestProperty(p, testType)).Select(p =>
                {
                    var (propertyInfo, traits) = ConcatTraits(p, t.traits, traitType);
                    return new TestMetadata(propertyInfo, traits);
                }));
        }

        private static int? GetFlUnitAbstractionsReferenceMajorVersion(Assembly assembly)
        {
            return assembly
                .GetReferencedAssemblies()
                .SingleOrDefault(n => n.Name == AbstractionsAssemblyName.Name)?.Version.Major;
        }

        private static (T memberInfo, IEnumerable<TraitAttribute> traits) ConcatTraits<T>(T memberInfo, IEnumerable<TraitAttribute> traits, Type traitAttributeType)
            where T : MemberInfo
        {
            return (memberInfo, traits: traits.Concat(GetTraits(memberInfo.GetCustomAttributesData(), traitAttributeType)));
        }

        private static IEnumerable<TraitAttribute> GetTraits(IList<CustomAttributeData> attributeData, Type traitAttributeType)
        {      
            return attributeData
                .Where(t => t.AttributeType == traitAttributeType)
                .Select(t =>
                {
                    var attributeName = (string)t.ConstructorArguments[0].Value;

                    if (t.ConstructorArguments.Count == 1)
                    {
                        return new TraitAttribute(attributeName);
                    }
                    else
                    {
                        return new TraitAttribute(attributeName, (string)t.ConstructorArguments[1].Value);
                    }
                });
        }

        private static bool IsTestProperty(PropertyInfo p, Type testType)
        {
            return p.CanRead
                && p.GetMethod.IsPublic
                && p.GetMethod.IsStatic
                && testType.IsAssignableFrom(p.PropertyType);
        }
    }
}
