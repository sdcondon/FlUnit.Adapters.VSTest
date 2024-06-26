﻿using System.Collections.Generic;
using System.Reflection;

namespace FlUnit.Adapters
{
    /// <summary>
    /// Container for test metadata - enough for the FlUnit execution logic to run a test.
    /// </summary>
    // TODO: design probably needs to change. on the discovery side, propertyinfo can't be used
    // outside of mlc, which makes this type a little too fragile. might need to split it?
    internal class TestMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestMetadata"/> class.
        /// </summary>
        /// <param name="propertyInfo">A <see cref="PropertyInfo"/> for the <see cref="Test"/>-valued property that represents the test.</param>
        /// <param name="traits">An enumerable of the traits that are applicable to this test.</param>
        public TestMetadata(PropertyInfo propertyInfo, IEnumerable<TraitAttribute> traits)
        {
            TestProperty = propertyInfo;
            Traits = traits;
        }

        /// <summary>
        /// Gets a <see cref="PropertyInfo"/> for the <see cref="Test"/>-valued property that represents the test.
        /// </summary>
        public PropertyInfo TestProperty { get; }

        /// <summary>
        /// Gets a serialisable representation of all of the information needed by FlUnit to run the test
        /// (notably, information about the property from which the test object can be retrieved).
        /// </summary>
        // Probably better to use JSON or similar - easier to future-proof..
        public string InternalData => $"{TestProperty.DeclaringType.Assembly.GetName().Name}:{TestProperty.DeclaringType.FullName}:{TestProperty.Name}";

        /// <summary>
        /// Gets an enumerable of the traits that are applicable to this test.
        /// </summary>
        public IEnumerable<TraitAttribute> Traits { get; }

        /// <summary>
        /// Loads a new instance of the <see cref="TestMetadata"/> class from serialized information.
        /// Loads the relevant assembly if necessary.
        /// </summary>
        /// <param name="internalData">FlUnit's internal data for the test (as provided by the <see cref="InternalData"/> property of another instance).</param>
        /// <param name="traits">An enumerable of the traits that are applicable to this test.</param>
        public static TestMetadata Load(string internalData, IEnumerable<TraitAttribute> traits)
        {
            var propertyDetails = internalData.Split(':');
            var assembly = Assembly.Load(propertyDetails[0]); // Might already be loaded - not sure of best practices here.
            var type = assembly.GetType(propertyDetails[1]);
            return new TestMetadata(type.GetProperty(propertyDetails[2]), traits);
        }
    }
}
