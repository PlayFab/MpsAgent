namespace Microsoft.Azure.Gaming.VmAgent.Model
{
    public interface IPersistedState
    {
        /// <summary>
        /// Like ToString(), but without any secrets/credentials.
        /// Used to summarize the state for logs.
        /// </summary>
        public string ToRedactedString();
    }
}
