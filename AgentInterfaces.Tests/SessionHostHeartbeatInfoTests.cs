// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace AgentInterfaces.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Azure.Gaming.AgentInterfaces;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SessionHostHeartbeatInfoTests
    {
        [TestMethod]
        [TestCategory("BVT")]
        public void TestIsStateSameForDifferentServerState()
        {
            SessionHostHeartbeatInfo original = CreateSessionHostHeartbeatInfo();
            original.CurrentGameState = SessionHostStatus.Active;
            SessionHostHeartbeatInfo copy = CreateSessionHostHeartbeatInfo();
            CopyState(original, copy);
            Assert.IsTrue(original.IsStateSame(copy));

            copy.CurrentGameState = SessionHostStatus.Terminating;
            Assert.IsFalse(original.IsStateSame(copy));
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void TestIsStateSameForDifferentServerHealth()
        {
            SessionHostHeartbeatInfo original = CreateSessionHostHeartbeatInfo();
            original.CurrentGameHealth = SessionHostHealth.Unhealthy;
            SessionHostHeartbeatInfo copy = CreateSessionHostHeartbeatInfo();
            CopyState(original, copy);
            Assert.IsTrue(original.IsStateSame(copy));

            copy.CurrentGameHealth = SessionHostHealth.Healthy;
            Assert.IsFalse(original.IsStateSame(copy));
        }

        [TestMethod]
        [TestCategory("BVT")]
        public void TestIsStateSameForDifferentConnectedPlayers()
        {
            // Make an original heartbeat
            SessionHostHeartbeatInfo original = CreateSessionHostHeartbeatInfo();
            SessionHostHeartbeatInfo copy = CreateSessionHostHeartbeatInfo();
            CopyState(original, copy);
            Assert.IsTrue(original.IsStateSame(copy));

            copy.CurrentPlayers = original.CurrentPlayers.Concat(GetConnectedPlayers()).ToList();
            Assert.IsFalse(original.IsStateSame(copy));
        }

        private void CopyState(SessionHostHeartbeatInfo original, SessionHostHeartbeatInfo copy)
        {
            copy.CurrentGameState = original.CurrentGameState;
            copy.CurrentPlayers = original.CurrentPlayers;
            copy.CurrentGameHealth = original.CurrentGameHealth;
        }

        private List<PortMapping> GetPortMapping(string name, int port, string protocol)
        {
            return new List<PortMapping>
            {
                new PortMapping
                {
                    PublicPort = 1000,
                    NodePort = 1000,
                    GamePort = new Port(name, port, protocol)
                }
            };
        }

        private SessionHostHeartbeatInfo CreateSessionHostHeartbeatInfo(List<PortMapping> portMappings = null, List<ConnectedPlayer> connectedPlayers = null)
        {
            return new SessionHostHeartbeatInfo
            {
                AssignmentId = "originalAssignmentId",
                NextScheduledMaintenanceUtc = DateTime.UtcNow,
                NextHeartbeatIntervalMs = 10,
                CurrentGameState = SessionHostStatus.Active,
                CurrentGameHealth = SessionHostHealth.Healthy,
                Operation = Operation.Active,
                SessionConfig = new SessionConfig
                {
                    SessionId = Guid.NewGuid(),
                    SessionCookie = "originalSessionCookie"
                },
                PortMappings = portMappings ?? GetPortMapping("1000", 1000, "TCP"),
                CurrentPlayers = connectedPlayers ?? GetConnectedPlayers()
            };
        }

        private List<ConnectedPlayer> GetConnectedPlayers(List<string> playerIds = null)
        {
            if (playerIds?.Count > 0)
            {
                return playerIds.Select(x => new ConnectedPlayer { PlayerId = x }).ToList();
            }

            return Enumerable.Range(0, 10).Select(x => new ConnectedPlayer { PlayerId = $"Player{x}" }).ToList();
        }
    }
}
