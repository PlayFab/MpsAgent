using System;
using System.Collections.Generic;
using System.Text;

namespace VmAgent.Core.UnitTests
{
    using FluentAssertions;
    using Microsoft.Azure.Gaming.VmAgent.Core;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class SerializationTests
    {
        [TestMethod]
        [TestCategory("BVT")]
        public void JsonSerializeDoesNotClobberDictionaryKeys()
        {
            var sampleObject = new SampleObjectWithDictionary()
            {
                Metadata = new Dictionary<string, string>()
                {
                    {"KEYWITHALLCAPS", "value1"},
                    {"keywithallsmallcase", "value2" },
                    {"keyWithMixedCase", "value3" }
                }
            };

            string serializedValue = JsonConvert.SerializeObject(sampleObject, CommonSettings.JsonSerializerSettings);
            SampleObjectWithDictionary deserializedObject =
                JsonConvert.DeserializeObject<SampleObjectWithDictionary>(serializedValue, CommonSettings.JsonSerializerSettings);
            deserializedObject.Should().BeEquivalentTo(sampleObject);
        }

        private class SampleObjectWithDictionary
        {
            public IDictionary<string, string> Metadata { get; set; }
        }
    }
}
