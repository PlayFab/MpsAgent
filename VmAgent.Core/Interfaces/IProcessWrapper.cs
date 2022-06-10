// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Core.Interfaces
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;

    public interface IProcessWrapper
    {
        void Kill(int id);

        int Start(ProcessStartInfo startInfo);

        void StartErrorStreamReader(Process process);

        StreamReader StandardErrorReader { get; }

        IEnumerable<int> List();

        void WaitForProcessExit(int id);
    }
}
