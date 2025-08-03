using UnityEngine;

namespace BlendShapePresets.Editor
{
    /// <summary>
    /// Logging utility class for BlendShapePresets project
    /// Provides unified log format and prefix
    /// </summary>
    public static class BlendShapeLogger
    {
        private const string LOG_PREFIX = "[BlendShapePresets]";

        /// <summary>
        /// Outputs an information level log
        /// </summary>
        /// <param name="message">Log message</param>
        public static void Log(string message)
        {
            Debug.Log($"{LOG_PREFIX} {message}");
        }

        /// <summary>
        /// Outputs a warning level log
        /// </summary>
        /// <param name="message">Log message</param>
        public static void LogWarning(string message)
        {
            Debug.LogWarning($"{LOG_PREFIX} {message}");
        }

        /// <summary>
        /// Outputs an error level log
        /// </summary>
        /// <param name="message">Log message</param>
        public static void LogError(string message)
        {
            Debug.LogError($"{LOG_PREFIX} {message}");
        }

        /// <summary>
        /// Outputs an information level log (with format string support)
        /// </summary>
        /// <param name="format">Format string</param>
        /// <param name="args">Format arguments</param>
        public static void LogFormat(string format, params object[] args)
        {
            Debug.Log($"{LOG_PREFIX} {string.Format(format, args)}");
        }

        /// <summary>
        /// Outputs a warning level log (with format string support)
        /// </summary>
        /// <param name="format">Format string</param>
        /// <param name="args">Format arguments</param>
        public static void LogWarningFormat(string format, params object[] args)
        {
            Debug.LogWarning($"{LOG_PREFIX} {string.Format(format, args)}");
        }

        /// <summary>
        /// Outputs an error level log (with format string support)
        /// </summary>
        /// <param name="format">Format string</param>
        /// <param name="args">Format arguments</param>
        public static void LogErrorFormat(string format, params object[] args)
        {
            Debug.LogError($"{LOG_PREFIX} {string.Format(format, args)}");
        }

        /// <summary>
        /// Outputs a completion log with processing time
        /// </summary>
        /// <param name="operation">Operation name</param>
        /// <param name="durationMs">Processing time in milliseconds</param>
        /// <param name="additionalInfo">Additional information (optional)</param>
        public static void LogCompletion(string operation, double durationMs, string additionalInfo = null)
        {
            var message = $"{operation}: Completed in {durationMs:F2}ms";
            if (!string.IsNullOrEmpty(additionalInfo))
            {
                message += $". {additionalInfo}";
            }
            Log(message);
        }

        /// <summary>
        /// Outputs an error log with exception information
        /// </summary>
        /// <param name="operation">Operation name</param>
        /// <param name="exception">Exception object</param>
        public static void LogException(string operation, System.Exception exception)
        {
            LogError($"{operation}: {exception.Message}");
        }

        /// <summary>
        /// Outputs an operation start log
        /// </summary>
        /// <param name="operation">Operation name</param>
        /// <param name="target">Target object name (optional)</param>
        /// <param name="parameters">Parameter information (optional)</param>
        public static void LogStart(string operation, string target = null, string parameters = null)
        {
            var message = $"{operation}: Starting";
            if (!string.IsNullOrEmpty(target))
            {
                message += $" for '{target}'";
            }
            if (!string.IsNullOrEmpty(parameters))
            {
                message += $" ({parameters})";
            }
            Log(message);
        }
    }
}