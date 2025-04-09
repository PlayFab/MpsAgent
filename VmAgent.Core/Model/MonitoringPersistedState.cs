namespace Microsoft.Azure.Gaming.VmAgent.Model
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;

    public enum MonitoringApplicationState
    {
        Initializing,
        Installing,
        InstallationFailure,
        Installed,
        Executing,
        ExecutionFailure,
        SignaledToStop,
        ExecutionComplete
    }
    public class MonitoringPersistedState: IPersistedState
    {
        public int MonitoringPid { get; set; }
        public MonitoringApplicationState MonitoringState { get; set; }
        public DateTime ExpectedCompletionTime { get; set; } 
        public string CallerSessionId { get; set; }
        public string OutputId { get; set; }

        public string ToRedactedString()
        {
            // This class has nothing to redact
            return JsonSerializer.Serialize(this);
        }
    }
}
