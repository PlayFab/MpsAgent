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
        ExecutionComplete
    }
    public class MonitoringPersistedState
    {
        public int MonitoringPID { get; set; }
        public int RetryCount { get; set; }
        public MonitoringApplicationState MonitoringState { get; set; }
        public DateTime ApplicationStartTime { get; set; } 
    }
}
