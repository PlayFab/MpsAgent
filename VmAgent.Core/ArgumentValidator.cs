// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Core
{
    using System;

    public static class ArgumentValidator
    {
        public static void ThrowIfNull(object argument, string argumentName)
        {
            if (argument == null)
            {
                throw new ArgumentNullException($"{argumentName} can not be null");
            }
        }

        public static void ThrowIfNullOrEmpty(string argument, string argumentName)
        {
            if (argument == null)
            {
                throw new ArgumentNullException($"{argumentName} can not be null");
            }

            if (argument.Length == 0)
            {
                throw new ArgumentException($"{argumentName} can not be empty");
            }
        }
    }
}
