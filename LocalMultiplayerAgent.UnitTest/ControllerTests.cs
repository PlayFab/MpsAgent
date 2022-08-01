// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.UnitTests
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Net.Http;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using AgentInterfaces;
    using Core.Interfaces;
    using FluentAssertions;
    using LocalMultiplayerAgent.Config;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Azure.Gaming.LocalMultiplayerAgent;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ControllerTests
    {

        private TestServer server;
        protected HttpClient Client { get; private set; }

        [TestMethod]
        [TestCategory("BVT")]
        public async Task ValidateStandingByToInitializingThrows()
        {
            this.server = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>());
            this.Client = this.server.CreateClient();

            string sessionHostId = "testSessionHostId";

            // send the first heartbeat with "StandingBy"
            SessionHostHeartbeatInfo heartbeat = new SessionHostHeartbeatInfo()
            {
                CurrentGameState = SessionHostStatus.StandingBy
            };
            var json = JsonConvert.SerializeObject(heartbeat);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var value = await this.Client.PostAsync($"v1/sessionHosts/{sessionHostId}/heartbeats", data);

            Assert.IsTrue(value.IsSuccessStatusCode);

            // send the first heartbeat with "StandingBy"
            heartbeat.CurrentGameState = SessionHostStatus.Initializing;
            json = JsonConvert.SerializeObject(heartbeat);
            data = new StringContent(json, Encoding.UTF8, "application/json");
            
            value = await this.Client.PostAsync($"v1/sessionHosts/{sessionHostId}/heartbeats", data);
            Assert.IsFalse(value.IsSuccessStatusCode);
            Assert.IsTrue(value.StatusCode == System.Net.HttpStatusCode.BadRequest);
        }
    }
}