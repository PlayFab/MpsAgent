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

        public void LogVerbose(string message, bool sanitize = false)
        {
            message = SanitizeMessage(message, sanitize);
            _logger.LogInformation(message);
            _genevaOtelLogger?.LogInformation(message);
        }

        public void LogInformation(string message, bool sanitize = false)
        {
            message = SanitizeMessage(message, sanitize);
            _logger.LogInformation(message);
            _genevaOtelLogger?.LogInformation(message);
        }

        public void LogWarning(string message, bool sanitize = false)
        {
            message = SanitizeMessage(message, sanitize);
            _logger.LogWarning(message);
            _genevaOtelLogger?.LogWarning(message);
        }

        public void LogError(string message, bool sanitize = false)
        {
            message = SanitizeMessage(message, sanitize);
            _logger.LogError(message);
            _genevaOtelLogger?.LogError(message);
        }

        public void LogException(Exception exception, bool sanitize = false)
        {
            string message = SanitizeMessage(exception.ToString(), sanitize);
            _logger.LogError(message);
            _genevaOtelLogger?.LogError(message);
        }

        public void LogException(string message, Exception exception, bool sanitize = false)
        {
            string logMessage = SanitizeMessage($"{message}. Exception: {exception}", sanitize);
            _logger.LogError(logMessage);
            _genevaOtelLogger?.LogError(logMessage);
        }

        public void LogEvent(string eventName, IDictionary<string, string> properties, IDictionary<string, double> metrics, bool sanitize = false)
        {
            string propertiesString = properties == null ? CommonSettings.NullStringValue : JsonConvert.SerializeObject(properties, CommonSettings.JsonSerializerSettings);
            string metricsString = metrics == null ? CommonSettings.NullStringValue : JsonConvert.SerializeObject(metrics, CommonSettings.JsonSerializerSettings);
            string message = SanitizeMessage($"Event: {eventName}. Properties: {propertiesString}, Metrics: {metricsString}", sanitize);
            _logger.LogInformation(message);
            _genevaOtelLogger?.LogInformation(message);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            string message = formatter(state, exception);
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

        private string SanitizeMessage(string message, bool sanitize)
        {
            return sanitize ? LogSanitizer.Sanitize(message) : message;
        }
    }
}
