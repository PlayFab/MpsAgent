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

        public MultiLogger(ILogger logger, TelemetryClient genevaTelemetryClient, ILogger genevaOpenTelemetryLogger = null)
        {
            _logger = logger ?? throw new ArgumentException("logger cannot be null");

            // We have 2 ILogger parameters (1. Logger to write logs to local file. 2.Geneva OpenTelemetry Logger)
            // We will replace GenevaAgentchannel with OpenTelemetry, once we verify all logs both in test and prod environments.
            _genevaTelemetryClient = genevaTelemetryClient ?? throw new ArgumentException("genevaTelemetryClient cannot be null");
            _genevaOtelLogger = genevaOpenTelemetryLogger;
        }

        public void LogVerbose(string message)
        {
            _logger.LogInformation(message);
        }

        public void LogInformation(string message)
        {
            _logger.LogInformation(message);
            _genevaTelemetryClient.TrackTrace(message, SeverityLevel.Information);
            _genevaOtelLogger?.LogInformation(message);
        }

        public void LogWarning(string message)
        {
            _logger.LogWarning(message);
            _genevaTelemetryClient.TrackTrace(message, SeverityLevel.Warning);
            _genevaOtelLogger?.LogWarning(message);
        }

        public void LogError(string message)
        {
            _logger.LogError(message);
            _genevaTelemetryClient.TrackTrace(message, SeverityLevel.Error);
            _genevaOtelLogger?.LogError(message);
        }

        public void LogException(Exception exception)
        {
            string message = exception.ToString();
            _logger.LogError(message);
            _genevaTelemetryClient.TrackTrace(message, SeverityLevel.Error);
            _genevaOtelLogger?.LogError(message);
        }

        public void LogException(string message, Exception exception)
        {
            string logMessage = $"{message}. Exception: {exception}";
            _logger.LogError(logMessage);
            _genevaTelemetryClient.TrackTrace(logMessage, SeverityLevel.Error);
            _genevaOtelLogger?.LogError(logMessage);
        }

        public void LogEvent(string eventName, IDictionary<string, string> properties, IDictionary<string, double> metrics)
        {
            string propertiesString = properties == null ? CommonSettings.NullStringValue : JsonConvert.SerializeObject(properties, CommonSettings.JsonSerializerSettings);
            string metricsString = metrics == null ? CommonSettings.NullStringValue : JsonConvert.SerializeObject(metrics, CommonSettings.JsonSerializerSettings);
            string message = $"Event: {eventName}. Properties: {propertiesString}, Metrics: {metricsString}";
            _logger.LogInformation(message);
            _genevaOtelLogger?.LogInformation(message);
            // this used to be TrackEvent in AppInsights
            // we converted it to a trace since dgrep columns could increase significantly
            _genevaTelemetryClient.TrackTrace(message, SeverityLevel.Information);
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
