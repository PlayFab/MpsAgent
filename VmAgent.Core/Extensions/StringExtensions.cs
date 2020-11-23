// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Extensions
{
    using System;

    public static class StringExtensions
    {
        private const string NullStringValue = "NULL";

        public static string ToLogString(this object value)
        {
            return value?.ToString() ?? NullStringValue;
        }

        public static bool EqualsIgnoreCase(this string string1, string string2)
        {
            return string.Equals(string1, string2, StringComparison.OrdinalIgnoreCase);
        }
    }
}
