namespace VmAgent.Core.Model
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
        public MonitoringApplicationState MonitoringState { get; set; }
    }
}
