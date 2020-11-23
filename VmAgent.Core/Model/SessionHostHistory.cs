// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Model
{
    using System;

    public class SessionHostHistory
    {
        public DateTime Timestamp { get; set; }

        public bool ReachedStandingBy { get; set; }

        public int RestartCount { get; set; }
    }
}
