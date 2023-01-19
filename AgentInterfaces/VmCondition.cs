namespace Microsoft.Azure.Gaming.AgentInterfaces
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    [Serializable]
    public class VmCondition
    {
        [Required] 
        public string Condition { get; set; }
        [Required] 
        public DateTime LastStateTransitionTimestamp { get; set; }
        public string Details { get; set; }
    }
}