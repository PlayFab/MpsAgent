// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Core;
    using Core.Interfaces;

    public static class TaskUtil
    {
        /// <summary>
        /// Runs the specified action periodically (including amidst exceptions) until explicitly cancelled.
        /// </summary>
        /// <param name="action">The operation to perform periodically.</param>
        /// <param name="systemOperations"></param>
        /// <param name="intervalBetweenRuns">Delay between performing the operation (if the operation completes or if there is an exception).</param>
        /// <param name="cancellationToken">Mostly used for unit testing.</param>
        /// <returns></returns>
        public static async Task RunUntilCancelled(
            Func<Task> action,
            ISystemOperations systemOperations,
            TimeSpan intervalBetweenRuns,
            CancellationToken cancellationToken,
            MultiLogger logger)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await action();
                }
                catch (Exception e)
                {
                    logger.LogException(e);
                }

                try
                {
                    await systemOperations.Delay((int)intervalBetweenRuns.TotalMilliseconds, cancellationToken); 
                }
                catch (TaskCanceledException ex)
                {
                    logger.LogInformation($"{nameof(RunUntilCancelled)} stopped with {nameof(TaskCanceledException)}: {ex}");
                }
            }
        }

        public static async Task<T> TimedExecute<T>(Func<Task<T>> action, MultiLogger logger, string eventName = null, string metricName = null, long elapsedThreshold = -1)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            T result = await action();

            if (!string.IsNullOrEmpty(eventName))
            {
                long elapsedMs = stopwatch.ElapsedMilliseconds;
                if (elapsedMs > elapsedThreshold)
                {
                    logger.LogEvent(eventName, null, new Dictionary<string, double>() { { metricName, elapsedMs }, });
                }
            }

            return result;
        }
    }
}
