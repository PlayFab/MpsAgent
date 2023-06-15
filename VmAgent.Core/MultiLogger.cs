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
        private ILogger _genevaOTelLogger;
        private const string VmAgentLoggerName = "VmAgentLogger";

        public MultiLogger(ILogger logger, TelemetryClient genevaTelemetryClient, ILogger genevaLogger = null)
        {
            _logger = logger ?? throw new ArgumentException("logger cannot be null");

            // To enable both GenevaAgentChannel and OpenTelemetry, we have 2 ILogger parameters here.
            // We will replace GenevaAgentchannel with Opentelemetry, once we verify all logs both in test and prod environments.
            _genevaTelemetryClient = genevaTelemetryClient ?? throw new ArgumentException("genevaTelemetryClient cannot be null");
            _genevaOTelLogger = genevaLogger;
        }

        public void SetUpLogger(ILoggerFactory loggerFactory)
        {
            _genevaOTelLogger = loggerFactory.CreateLogger(VmAgentLoggerName);
        }

        public void LogVerbose(string message)
        {
            _logger.LogInformation(message);
        }

        public void LogInformation(string message)
        {
            _logger.LogInformation(message);
            _genevaTelemetryClient.TrackTrace(message, SeverityLevel.Information);
            _genevaOTelLogger?.LogInformation(message);
        }

        public void LogWarning(string message)
        {
            _logger.LogWarning(message);
            _genevaTelemetryClient.TrackTrace(message, SeverityLevel.Warning);
            _genevaOTelLogger?.LogWarning(message);
        }

        public void LogError(string message)
        {
            _logger.LogError(message);
            _genevaTelemetryClient.TrackTrace(message, SeverityLevel.Error);
            _genevaOTelLogger?.LogError(message);
        }

        public void LogException(Exception exception)
        {
            string message = exception.ToString();
            _logger.LogError(message);
            _genevaTelemetryClient.TrackTrace(message, SeverityLevel.Error);
            _genevaOTelLogger?.LogError(message);
        }

        public void LogException(string message, Exception exception)
        {
            string logMessage = $"{message}. Exception: {exception}";
            _logger.LogError(logMessage);
            _genevaTelemetryClient.TrackTrace(logMessage, SeverityLevel.Error);
            _genevaOTelLogger?.LogError(logMessage);
        }

        public void LogEvent(string eventName, IDictionary<string, string> properties, IDictionary<string, double> metrics)
        {
            string propertiesString = properties == null ? CommonSettings.NullStringValue : JsonConvert.SerializeObject(properties, CommonSettings.JsonSerializerSettings);
            string metricsString = metrics == null ? CommonSettings.NullStringValue : JsonConvert.SerializeObject(metrics, CommonSettings.JsonSerializerSettings);
            string message = $"Event: {eventName}. Properties: {propertiesString}, Metrics: {metricsString}";
            _logger.LogInformation(message);
            _genevaOTelLogger?.LogInformation(message);
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
