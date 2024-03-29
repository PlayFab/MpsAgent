// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.UnitTests
{
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using AgentInterfaces;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Azure.Gaming.LocalMultiplayerAgent;
    using Newtonsoft.Json;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ControllerTests
    {
        private static readonly string jsonMediaType = "application/json";
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

            Globals.Settings = new();

            // send the first heartbeat with "StandingBy"
            SessionHostHeartbeatInfo heartbeat = new SessionHostHeartbeatInfo()
            {
                CurrentGameState = SessionHostStatus.StandingBy
            };
            string json = JsonConvert.SerializeObject(heartbeat);
            StringContent data = new StringContent(json, Encoding.UTF8, jsonMediaType);
            HttpResponseMessage value = await this.Client.PostAsync($"v1/sessionHosts/{sessionHostId}/heartbeats", data);

            Assert.IsTrue(value.IsSuccessStatusCode);

            // send the second heartbeat with "Initializing"
            heartbeat.CurrentGameState = SessionHostStatus.Initializing;
            json = JsonConvert.SerializeObject(heartbeat);
            data = new StringContent(json, Encoding.UTF8, jsonMediaType);
            
            value = await this.Client.PostAsync($"v1/sessionHosts/{sessionHostId}/heartbeats", data);
            Assert.IsFalse(value.IsSuccessStatusCode);
            Assert.IsTrue(value.StatusCode == System.Net.HttpStatusCode.BadRequest);
        }
    }
}