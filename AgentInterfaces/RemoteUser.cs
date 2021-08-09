namespace Microsoft.Azure.Gaming.AgentInterfaces
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class RemoteUser
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        public DateTime ExpirationTime { get; set; }
    }
}