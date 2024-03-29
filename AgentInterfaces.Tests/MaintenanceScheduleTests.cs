// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AgentInterfaces.Tests
{
    using System.IO;
    using Microsoft.Azure.Gaming.AgentInterfaces;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class MaintenanceScheduleTests
    {
        [TestMethod]
        [TestCategory("BVT")]
        public void TestParseSingleMaintenanceEvent()
        {
            string payload = File.ReadAllText("SingleMaintenanceEvent.json");
            MaintenanceSchedule schedule = JsonConvert.DeserializeObject<MaintenanceSchedule>(payload);
            Assert.AreEqual("1", schedule.DocumentIncarnation);
            Assert.AreEqual(1, schedule.MaintenanceEvents.Count);
            Assert.AreEqual(1, schedule.MaintenanceEvents[0].AffectedResources.Count);
            Assert.AreEqual(10, schedule.MaintenanceEvents[0].DurationInSeconds);
            AssertNonNullProperties(schedule);
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void TestParseMultipleMaintenanceEvents()
        {
            string payload = File.ReadAllText("MultipleMaintenanceEvents.json");
            MaintenanceSchedule schedule = JsonConvert.DeserializeObject<MaintenanceSchedule>(payload);
            Assert.AreEqual("1", schedule.DocumentIncarnation);
            Assert.AreEqual(2, schedule.MaintenanceEvents.Count);
            Assert.AreEqual(1, schedule.MaintenanceEvents[0].AffectedResources.Count);
            Assert.AreEqual(9, schedule.MaintenanceEvents[0].DurationInSeconds);
            Assert.AreEqual(-1, schedule.MaintenanceEvents[1].DurationInSeconds);
            AssertNonNullProperties(schedule);
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void TestParseEmptyMaintenanceEvent()
        {
            string payload = File.ReadAllText("EmptyMaintenanceEvent.json");
            MaintenanceSchedule schedule = JsonConvert.DeserializeObject<MaintenanceSchedule>(payload);
            Assert.AreEqual("1", schedule.DocumentIncarnation);
            Assert.AreEqual(0, schedule.MaintenanceEvents.Count);
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void TestParseSingleMaintenanceEventMultipleAffectedResources()
        {
            string payload = File.ReadAllText("SingleMaintenanceEventMultipleAffectedResources.json");
            MaintenanceSchedule schedule = JsonConvert.DeserializeObject<MaintenanceSchedule>(payload);
            Assert.AreEqual("1", schedule.DocumentIncarnation);
            Assert.AreEqual(1, schedule.MaintenanceEvents.Count);
            Assert.AreEqual(2, schedule.MaintenanceEvents[0].AffectedResources.Count);
            AssertNonNullProperties(schedule);
        }

        private static void AssertNonNullProperties(MaintenanceSchedule maintenanceSchedule)
        {
            Assert.IsNotNull(maintenanceSchedule.MaintenanceEvents[0].EventId);
            Assert.IsNotNull(maintenanceSchedule.MaintenanceEvents[0].EventStatus);
            Assert.IsNotNull(maintenanceSchedule.MaintenanceEvents[0].EventType);
            Assert.IsNotNull(maintenanceSchedule.MaintenanceEvents[0].NotBefore);
            Assert.IsNotNull(maintenanceSchedule.MaintenanceEvents[0].EventSource);
        }
    }
}
