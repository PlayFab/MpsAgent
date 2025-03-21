// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace Microsoft.Azure.Gaming.VmAgent.Core
{
    public class MultiLogger : ILogger
    {
        private readonly ILogger _logger;
        private readonly ILogger _genevaOtelLogger;

        public MultiLogger(ILogger logger, ILogger genevaOpenTelemetryLogger = null)
        {
            _logger = logger ?? throw new ArgumentException("logger cannot be null");
            _genevaOtelLogger = genevaOpenTelemetryLogger;
        }

        public void LogVerbose(string message)
        {
            message = LogSanitizer.Sanitize(message);
            _logger.LogInformation(message);
            _genevaOtelLogger?.LogInformation(message);
        }

        public void LogInformation(string message)
        {
            message = LogSanitizer.Sanitize(message);
            _logger.LogInformation(message);
            _genevaOtelLogger?.LogInformation(message);
        }

        public void LogWarning(string message)
        {
            message = LogSanitizer.Sanitize(message);
            _logger.LogWarning(message);
            _genevaOtelLogger?.LogWarning(message);
        }

        public void LogError(string message)
        {
            message = LogSanitizer.Sanitize(message);
            _logger.LogError(message);
            _genevaOtelLogger?.LogError(message);
        }

        public void LogException(Exception exception)
        {
            string message = LogSanitizer.Sanitize(exception.ToString());
            _logger.LogError(message);
            _genevaOtelLogger?.LogError(message);
        }

        public void LogException(string message, Exception exception)
        {
            string logMessage = LogSanitizer.Sanitize($"{message}. Exception: {exception}");
            _logger.LogError(logMessage);
            _genevaOtelLogger?.LogError(logMessage);
        }

        public void LogEvent(string eventName, IDictionary<string, string> properties, IDictionary<string, double> metrics)
        {
            string propertiesString = properties == null ? CommonSettings.NullStringValue : JsonConvert.SerializeObject(properties, CommonSettings.JsonSerializerSettings);
            string metricsString = metrics == null ? CommonSettings.NullStringValue : JsonConvert.SerializeObject(metrics, CommonSettings.JsonSerializerSettings);
            string message = LogSanitizer.Sanitize($"Event: {eventName}. Properties: {propertiesString}, Metrics: {metricsString}");
            _logger.LogInformation(message);
            _genevaOtelLogger?.LogInformation(message);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            string message = LogSanitizer.Sanitize(formatter(state, exception));
            _logger.Log(logLevel, eventId, state, exception, (s, e) => message);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _logger.IsEnabled(logLevel);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return _logger.BeginScope(state);
        }
    }
}
