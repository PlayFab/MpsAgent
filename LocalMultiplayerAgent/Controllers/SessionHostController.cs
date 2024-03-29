// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace LocalMultiplayerAgent.Controllers
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Gaming.AgentInterfaces;
    using Microsoft.Azure.Gaming.LocalMultiplayerAgent;
    using Microsoft.Azure.Gaming.VmAgent.Model;
    using Microsoft.Extensions.Hosting;
    using Instrumentation;
    using System.Collections.Generic;

    public class SessionHostController : Controller
    {
        private const int DefaultHeartbeatIntervalMs = 1000;
        private static readonly ConcurrentDictionary<string, int> HeartBeatsCount = new ConcurrentDictionary<string, int>();
        private static SessionHostStatus previousSessionHostStatus = SessionHostStatus.Invalid;

        IHostApplicationLifetime applicationLifetime;
        public SessionHostController(IHostApplicationLifetime appLifetime)
        {
            applicationLifetime = appLifetime;
        }

        [HttpPost]
        [Route("v1/titles/{titleId}/clusters/{sessionHost}/instances/{instanceId}/heartbeat")]
        [Route("v1/titles/{titleId}/sessionHost/{sessionHost}/instances/{instanceId}/heartbeat")]
        public async Task<IActionResult> ProcessHeartbeatLegacy(string titleId, string sessionHost, string instanceId,
            [FromBody] LegacyGameInfo heartbeatRequest)
        {

            // Do all the work with the newer GameInfo object, and just convert back to a LegacyGameInfo before responding
            IActionResult result = await ProcessHeartbeat(instanceId, heartbeatRequest.ToSessionHostHeartbeatInfo());

            if (result is OkObjectResult response)
            {
                LegacyGameInfo legacyResponse = LegacyGameInfo.FromSessionHostHeartbeatInfo(response.Value as SessionHostHeartbeatInfo, titleId);
                response.Value = legacyResponse;
            }

            return result;
        }

        [HttpPost]
        [Route("v1/sessionHosts/{sessionHostId}/heartbeats")]
        public async Task<IActionResult> ProcessHeartbeat(string sessionHostId,
            [FromBody] SessionHostHeartbeatInfo heartbeatRequest)
        {
            await Task.Delay(1);
            SessionHostStatus currentGameState = heartbeatRequest.CurrentGameState;
            Operation op = Operation.Continue;
            SessionConfig config = null;
            Console.WriteLine($"CurrentGameState: {heartbeatRequest.CurrentGameState}");
            bool sendMaintenanceEvent = false;

            if(!ValidateSessionHostStatusTransition(previousSessionHostStatus, currentGameState))
            {
                string errorMsg = $"Invalid transition from current status: {previousSessionHostStatus} to status: {heartbeatRequest.CurrentGameState}";
                applicationLifetime.StopApplication();
                return BadRequest(errorMsg);
            }

            if (currentGameState == SessionHostStatus.Terminated || currentGameState == SessionHostStatus.Terminating)
            {
                HeartBeatsCount.TryRemove(sessionHostId, out _);
            }
            else if (HeartBeatsCount.TryGetValue(sessionHostId, out int numHeartBeats))
            {
                if (numHeartBeats >= Globals.Settings.NumHeartBeatsForTerminateResponse)
                {
                    op = Operation.Terminate;
                }
                else if (numHeartBeats >= Globals.Settings.NumHeartBeatsForActivateResponse && currentGameState == SessionHostStatus.StandingBy)
                {
                    op = Operation.Active;
                    config = Globals.SessionConfig;
                }

                sendMaintenanceEvent = (numHeartBeats == Globals.Settings.NumHeartBeatsForMaintenanceEventResponse);

                HeartBeatsCount[sessionHostId]++;
            }
            else
            {
                sendMaintenanceEvent = (Globals.Settings.NumHeartBeatsForMaintenanceEventResponse == 1);
                HeartBeatsCount.TryAdd(sessionHostId, 1);
            }

            previousSessionHostStatus = currentGameState;

            SessionHostHeartbeatInfo heartbeatResponse = new SessionHostHeartbeatInfo
            {
                CurrentGameState = currentGameState,
                NextHeartbeatIntervalMs = DefaultHeartbeatIntervalMs,
                Operation = op,
                SessionConfig = config
            };

            if (_wasGsdkVersionLogged && sendMaintenanceEvent)
            {
                heartbeatResponse.MaintenanceSchedule = new()
                {
                    DocumentIncarnation = "1",
                    MaintenanceEvents = new List<MaintenanceEvent>()
                    {
                        new()
                        {
                            EventId = Guid.NewGuid().ToString(),
                            EventType = Globals.Settings.MaintenanceEventType,
                            ResourceType = "VirtualMachine",
                            AffectedResources = new List<string>() { "vmId" },
                            EventStatus = Globals.Settings.MaintenanceEventStatus,
                            NotBefore = DateTime.UtcNow.AddMinutes(5),
                            Description = $"Scheduled {Globals.Settings.MaintenanceEventType} event for VM",
                            EventSource = Globals.Settings.MaintenanceEventSource,
                            DurationInSeconds = 300
                        }
                    }
                };
            }

            return Ok(heartbeatResponse);
        }

        private static bool _wasGsdkVersionLogged = false;
        [HttpPost]
        [Route("v1/metrics/{sessionHostId}/gsdkinfo")]
        public IActionResult ReportGsdkVersion(string sessionHostId, [FromBody] GsdkVersionInfo vi)
        {
            if (string.IsNullOrEmpty(vi.Version))
            {
                return BadRequest($"{nameof(GsdkVersionInfo.Version)} should not be an empty string");
            }
            if (string.IsNullOrEmpty(vi.Flavor))
            {
                return BadRequest($"{nameof(GsdkVersionInfo.Flavor)} should not be an empty string");
            }

            if (!_wasGsdkVersionLogged)
            {
                Globals.MultiLogger.LogInformation($"GSDK flavor/version: {vi.Flavor}/{vi.Version} from session host {sessionHostId}");
                _wasGsdkVersionLogged = true;
            }

            return Ok();
        }

        [HttpPatch]
        [Route("v1/sessionHosts/{sessionHostId}")]
        public Task<IActionResult> ProcessHeartbeatV1(
            string sessionHostId,
            [FromBody] SessionHostHeartbeatInfo heartbeatRequest)
        {
            // To be removed once we update the new GSDK
            return ProcessHeartbeat(sessionHostId, heartbeatRequest);
        }

        /// <summary>
        /// Validates if the session host status originating from the heartbeat will result in a valid status transition
        /// </summary>
        /// <param name="previousStatus"></param>
        /// <param name="currentStatus"></param>
        /// <returns>if the transition is valid</returns>
        private bool ValidateSessionHostStatusTransition(SessionHostStatus previousStatus, SessionHostStatus currentStatus)
        {
            if(previousStatus == SessionHostStatus.StandingBy && currentStatus == SessionHostStatus.Initializing)
            {
                return false;
            }
            return true;
        }
    }
}
