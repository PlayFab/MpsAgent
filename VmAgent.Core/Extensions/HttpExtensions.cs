// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Extensions
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    public static class HttpExtensions
    {
        private static HttpMethod Patch { get; } = new HttpMethod("PATCH");

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "SendAsync takes care of disposing content.")]
        public static Task<HttpResponseMessage> PatchAsync(this HttpClient client, string requestUri, HttpContent content)
        {
            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = Patch,
                RequestUri = new Uri(requestUri),
                Content = content,
            };

            return client.SendAsync(request);
        }

        public static Task<HttpResponseMessage> PatchAsync(this HttpClient client, Uri requestUri, HttpContent content)
        {
            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = Patch,
                RequestUri = requestUri,
                Content = content,
            };

            return client.SendAsync(request);
        }
    }
}
