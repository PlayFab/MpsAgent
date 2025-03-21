// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;

namespace Microsoft.Azure.Gaming.VmAgent.Core
{
    public class LogSanitizer
    {
        private static readonly Regex SensitiveDataRegex1 = new Regex(@"(?i)(sig=|signature|key|token|secret|password|apikey|access_token)[^&\s]*", RegexOptions.Compiled);
        private static readonly Regex SensitiveDataRegex2 = new Regex(@"(?i)((se?cre?t(.?key)?|pa?s*wo?r?d(\w+)?\W|(primary|secondary)key|X509Certificates2?|credentials?|apikey|access_token)[\""\'\\`]*[\s,\(]*([\""\'`]? value[\""\'`]?\\s*)?[:=<>]+\s*[\""\'`]?[^\\s\""\'`\\$;]*)", RegexOptions.Compiled);

        public static string Sanitize(string message)
        {
            message = SensitiveDataRegex1.Replace(message, "[REDACTED]");
            message = SensitiveDataRegex2.Replace(message, "[REDACTED]");
            return message;
        }
    }
}