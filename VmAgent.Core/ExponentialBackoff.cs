// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Core
{
    using System;

    public class ExponentialBackoff
    {
        private readonly TimeSpan _minBackoff;
        private readonly TimeSpan _maxBackoff;

        public ExponentialBackoff(
            TimeSpan minBackoff,
            TimeSpan maxBackoff)
        {
            _minBackoff = minBackoff;
            _maxBackoff = maxBackoff;
        }

        public bool ShouldRetry(int currentRetryCount, DateTime lastRetryTime, DateTime currentTime)
        {
            return ShouldRetry(_minBackoff, _maxBackoff, currentRetryCount, lastRetryTime, currentTime);
        }

        public bool ShouldRetry(TimeSpan minBackoff, TimeSpan maxBackoff, int currentRetryCount, DateTime lastRetryTime, DateTime currentTime)
        {
            int backoffAfterMinMaxApplied;
            try
            {
                checked
                {
                    int backoffMs = (int)((Math.Pow(2.0, (double)currentRetryCount) - 1.0));
                    backoffMs = Math.Max(0, backoffMs);
                    backoffAfterMinMaxApplied = (int)Math.Min(minBackoff.TotalMilliseconds + backoffMs, maxBackoff.TotalMilliseconds);
                }
            }
            catch (OverflowException)
            {
                backoffAfterMinMaxApplied = (int)maxBackoff.TotalMilliseconds;
            }

            DateTime retryAfterTime = lastRetryTime + TimeSpan.FromMilliseconds((double)backoffAfterMinMaxApplied);

            return currentTime >= retryAfterTime;
        }
    }
}
