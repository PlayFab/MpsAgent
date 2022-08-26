// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Core.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    public interface IProcessWrapper
    {
        void Kill(int id);

        int Start(ProcessStartInfo startInfo);

        int StartWithEventHandler(ProcessStartInfo startInfo, Action<object, DataReceivedEventArgs> StdOutputHandler, Action<object, DataReceivedEventArgs> ErrorOutputHandler, Action<object, EventArgs> ProcessExitedHanlder);

        IEnumerable<int> List();

        void WaitForProcessExit(int id);
    }
}
