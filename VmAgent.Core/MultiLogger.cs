// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Azure.Gaming.VmAgent.Core
{
    using System;
    using System.Collections.Generic;
    using ApplicationInsights;
    using ApplicationInsights.DataContracts;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public class MultiLogger : ILogger
    {
        private readonly ILogger _logger;
        private readonly TelemetryClient _genevaTelemetryClient;
        private readonly ILogger _genevaOtelLogger;

        public MultiLogger(ILogger logger, ILogger genevaOpenTelemetryLogger = null)
        {
            _logger = logger ?? throw new ArgumentException("logger cannot be null");
            _genevaOtelLogger = genevaOpenTelemetryLogger;
        }

        public void LogVerbose(string message)
        {
            _logger.LogInformation(message);
            _genevaOtelLogger?.LogInformation(message);
        }

        public void LogInformation(string message)
        {
            _logger.LogInformation(message);
            _genevaOtelLogger?.LogInformation(message);
        }

        public void LogWarning(string message)
        {
            _logger.LogWarning(message);
            _genevaOtelLogger?.LogWarning(message);
        }

        public void LogError(string message)
        {
            _logger.LogError(message);
            _genevaOtelLogger?.LogError(message);
        }

        public void LogException(Exception exception)
        {
            string message = exception.ToString();
            _logger.LogError(message);
            _genevaOtelLogger?.LogError(message);
        }

        public void LogException(string message, Exception exception)
        {
            string logMessage = $"{message}. Exception: {exception}";
            _logger.LogError(logMessage);
            _genevaOtelLogger?.LogError(logMessage);
        }

        public void LogEvent(string eventName, IDictionary<string, string> properties, IDictionary<string, double> metrics)
        {
            string propertiesString = properties == null ? CommonSettings.NullStringValue : JsonConvert.SerializeObject(properties, CommonSettings.JsonSerializerSettings);
            string metricsString = metrics == null ? CommonSettings.NullStringValue : JsonConvert.SerializeObject(metrics, CommonSettings.JsonSerializerSettings);
            string message = $"Event: {eventName}. Properties: {propertiesString}, Metrics: {metricsString}";
            _logger.LogInformation(message);
            _genevaOtelLogger?.LogInformation(message);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _logger.Log(logLevel, eventId, state, exception, formatter);
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
