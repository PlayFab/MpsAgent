// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AgentInterfaces.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Azure.Gaming.AgentInterfaces;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [TestClass]
    public class DataModelTests
    {
        [TestMethod]
        [TestCategory("BVT")]
        public void AllEnumsAreMarkedForStringSerialization()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(VmState));
            IList<Type> enumTypes = assembly.GetTypes().Where(x => x.IsEnum).ToList();
            Assert.IsTrue(enumTypes.Count > 0);
            foreach (Type enumType in enumTypes)
            {
                JsonConverterAttribute converterAtrribute = enumType.GetCustomAttribute<JsonConverterAttribute>();
                if (converterAtrribute == null)
                {
                    Assert.Fail($"Attribute not set for {enumType.Name}.");
                }

                Assert.AreEqual(typeof(StringEnumConverter), converterAtrribute.ConverterType, $"Wrong converter set for {enumType.Name}.");
            }
        }
    }
}
