namespace Microsoft.Azure.Gaming.VmAgent.Model
{
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
        public int RetriyCount { get; set; }
        public MonitoringApplicationState MonitoringState { get; set; }
    }
}
