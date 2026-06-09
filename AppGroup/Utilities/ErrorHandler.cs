using AppGroup.Logging;
using System;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace AppGroup.Utilities
{
    /// <summary>
    /// Centralized error handler for the application.
    /// Provides consistent error handling, logging, and optional user feedback.
    /// </summary>
    public static class ErrorHandler
    {
        /// <summary>
        /// Global error handling strategy.
        /// </summary>
        public enum ErrorStrategy
        {
            /// <summary>Log only, continue execution</summary>
            LogAndContinue,
            /// <summary>Log and show message to user</summary>
            LogAndNotify,
            /// <summary>Log and re-throw</summary>
            LogAndRethrow,
            /// <summary>Silent fail (log only in debug)</summary>
            Silent
        }

        private static ErrorStrategy _defaultStrategy = ErrorStrategy.LogAndContinue;
        private static Action<string, Exception>? _onErrorCallback;

        /// <summary>
        /// Sets the default error handling strategy.
        /// </summary>
        public static void SetDefaultStrategy(ErrorStrategy strategy)
        {
            _defaultStrategy = strategy;
        }

        /// <summary>
        /// Sets a callback to be invoked when errors occur.
        /// Useful for showing error messages to the user.
        /// </summary>
        public static void SetErrorCallback(Action<string, Exception> callback)
        {
            _onErrorCallback = callback;
        }

        /// <summary>
        /// Handles an exception with the default strategy.
        /// </summary>
        /// <param name="ex">The exception to handle</param>
        /// <param name="context">Optional context message</param>
        /// <param name="strategy">Override default strategy</param>
        public static void Handle(Exception ex, string? context = null, ErrorStrategy? strategy = null)
        {
            var actualStrategy = strategy ?? _defaultStrategy;
            string message = BuildErrorMessage(ex, context);

            switch (actualStrategy)
            {
                case ErrorStrategy.LogAndContinue:
                    LogError(ex, context);
                    break;

                case ErrorStrategy.LogAndNotify:
                    LogError(ex, context);
                    NotifyUser(message, ex);
                    break;

                case ErrorStrategy.LogAndRethrow:
                    LogError(ex, context);
                    throw ex;

                case ErrorStrategy.Silent:
                    Logger.Debug(BuildErrorMessage(ex, context ?? "Silent error"));
                    break;
            }
        }

        /// <summary>
        /// Handles an exception and always continues (never throws).
        /// </summary>
        public static void HandleSilently(Exception ex, string? context = null)
        {
            Handle(ex, context, ErrorStrategy.LogAndContinue);
        }

        /// <summary>
        /// Handles an exception with the LogAndNotify strategy.
        /// </summary>
        public static void HandleWithNotification(Exception ex, string? context = null)
        {
            Handle(ex, context, ErrorStrategy.LogAndNotify);
        }

        /// <summary>
        /// Wraps an action with error handling.
        /// </summary>
        public static void Execute(Action action, string? context = null, ErrorStrategy strategy = ErrorStrategy.LogAndContinue)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Handle(ex, context, strategy);
            }
        }

        /// <summary>
        /// Wraps an async function with error handling.
        /// </summary>
        public static async Task ExecuteAsync(Func<Task> asyncAction, string? context = null, ErrorStrategy strategy = ErrorStrategy.LogAndContinue)
        {
            try
            {
                await asyncAction();
            }
            catch (Exception ex)
            {
                Handle(ex, context, strategy);
            }
        }

        /// <summary>
        /// Wraps a function with error handling and returns a default value on error.
        /// </summary>
        public static T ExecuteWithFallback<T>(Func<T> func, T defaultValue, string? context = null)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                Handle(ex, context);
                return defaultValue;
            }
        }

        /// <summary>
        /// Wraps an async function with error handling and returns a default value on error.
        /// </summary>
        public static async Task<T> ExecuteWithFallbackAsync<T>(Func<Task<T>> asyncFunc, T defaultValue, string? context = null)
        {
            try
            {
                return await asyncFunc();
            }
            catch (Exception ex)
            {
                Handle(ex, context);
                return defaultValue;
            }
        }

        /// <summary>
        /// Logs an error with full details.
        /// </summary>
        private static void LogError(Exception ex, string? context)
        {
            if (ex == null) return;

            string message = BuildErrorMessage(ex, context);

            // Log at appropriate level based on exception type
            if (ex is ArgumentException || ex is ArgumentNullException || ex is InvalidOperationException)
            {
                Logger.Warn(message);
            }
            else
            {
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// Notifies the user about an error.
        /// Currently uses Debug.WriteLine; can be extended to show message boxes.
        /// </summary>
        private static void NotifyUser(string message, Exception ex)
        {
            Debug.WriteLine($"ERROR: {message}");
            _onErrorCallback?.Invoke(message, ex);
        }

        /// <summary>
        /// Builds a comprehensive error message from an exception and optional context.
        /// </summary>
        private static string BuildErrorMessage(Exception ex, string? context)
        {
            if (ex == null) return context ?? "Unknown error";

            string message = ex.Message;
            if (!string.IsNullOrEmpty(context))
            {
                message = $"{context}: {message}";
            }

            return message;
        }

        /// <summary>
        /// Validates a condition and handles the error if validation fails.
        /// </summary>
        public static void Validate(bool condition, string errorMessage, ErrorStrategy strategy = ErrorStrategy.LogAndContinue)
        {
            if (!condition)
            {
                var ex = new InvalidOperationException(errorMessage);
                Handle(ex, null, strategy);
            }
        }

        /// <summary>
        /// Validates that a value is not null and handles the error if it is.
        /// </summary>
        public static void ValidateNotNull<T>(T? value, string paramName, ErrorStrategy strategy = ErrorStrategy.LogAndContinue) where T : class
        {
            if (value == null)
            {
                var ex = new ArgumentNullException(paramName);
                Handle(ex, $"Parameter {paramName} cannot be null", strategy);
            }
        }

        /// <summary>
        /// Validates that a string is not null or empty and handles the error if it is.
        /// </summary>
        public static void ValidateNotEmpty(string? value, string paramName, ErrorStrategy strategy = ErrorStrategy.LogAndContinue)
        {
            if (string.IsNullOrEmpty(value))
            {
                var ex = new ArgumentException($"Parameter {paramName} cannot be null or empty", paramName);
                Handle(ex, null, strategy);
            }
        }
    }
}
