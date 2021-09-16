namespace Microsoft.Azure.Gaming.VmAgent.Model
{
    using System;
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
    public class MonitoringPersistedState
    {
        public int MonitoringPid { get; set; }
        public MonitoringApplicationState MonitoringState { get; set; }
        public DateTime ExpectedCompletionTime { get; set; } 
        public string CallerSessionId { get; set; }
        public string OutputId { get; set; }
    }
}
