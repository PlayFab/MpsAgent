namespace Microsoft.Azure.Gaming.VmAgent.Model
{
    using System;
    public enum MonitoringApplicationState
    {
        Initializing,
        Downloading,
        DownloadFailure,
        Downloaded,
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
        public int MonitoringPID { get; set; }
        public MonitoringApplicationState MonitoringState { get; set; }
        public DateTime ExpectedCompletionTime { get; set; } 
        public string CallerSessionID { get; set; }
        public string OutputId { get; set; }
    }
}
