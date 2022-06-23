// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Core.Interfaces
{
    using System.Collections.Generic;
    using System.Diagnostics;

    public interface IProcessWrapper
    {
        void Kill(int id);

        int Start(ProcessStartInfo startInfo);

        IEnumerable<int> List();

        void WaitForProcessExit(int id);
    }
}
