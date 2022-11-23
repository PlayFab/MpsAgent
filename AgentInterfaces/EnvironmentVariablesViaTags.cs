namespace Microsoft.Azure.Gaming.AgentInterfaces
{
    // These are passed in as VMSS Tags (and set as environment variables by VmAgent Startup Task).
    public static class EnvironmentVariablesViaTags
    {
        public const string TitleId = "PF_MPS_TITLE_ID";
        public const string DeploymentId = "PF_MPS_DEPLOYMENT_ID";
        public const string CreationTime = "PF_MPS_CREATION_TIME";
        public const string Cluster = "PF_MPS_CLUSTER";
        public const string OsPlatform = "PF_MPS_OSPLATFORM";
        public const string Region = "PF_MPS_REGION";
        public const string VmSize = "PF_MPS_VM_SIZE";
        public const string VmFamily = "PF_MPS_VM_FAMILY";
        public const string SubscriptionId = "PF_MPS_SUBSCRIPTION_ID";
    }
}
