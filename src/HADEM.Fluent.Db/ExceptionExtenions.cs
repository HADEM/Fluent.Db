// Copyright (c) HADEM. All rights reserved.

namespace HADEM.Fluent.Db
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public static class ExceptionExtenions
    {
        /// <summary>
        /// Gets a list of all messages in the given exception.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <returns>A list of all messages.</returns>
        public static List<string> GetMessages(this Exception ex)
        {
            var exception = ex;
            var messages = new List<string>();

            while (exception != null)
            {
                messages.Add(exception.Message);
                exception = exception.InnerException;
            }

            return messages;
        }

        /// <summary>
        /// Returns the inner-most Exception.
        /// </summary>
        /// <param name="ex">The original exception.</param>
        /// <returns>The inner-most Exception.</returns>
        public static Exception GetInnerMostException(this Exception ex)
        {
            var exception = ex;
            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
            }

            return exception;
        }

        /// <summary>
        /// Get full stack trace message with the exception message.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <returns>Error message detail.</returns>
        public static string GetFullStackTraceWithMessage(this Exception ex)
        {
            var stackTrace = new StringBuilder();

            stackTrace.AppendLine(ex.Message);
            stackTrace.AppendLine(GetFullStackTrace(ex));
            return stackTrace.ToString();
        }

        /// <summary>
        /// Get full stack trace message.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <returns>Error message detail.</returns>
        public static string GetFullStackTrace(this Exception ex)
        {
            var stackTrace = new StringBuilder();

            stackTrace.AppendLine(ex.GetType().FullName);
            stackTrace.AppendLine(ex.StackTrace);

            for (var exception = ex.InnerException; exception != null; exception = exception.InnerException)
            {
                stackTrace.AppendLine("- Caused by: " + exception.Message);
                stackTrace.AppendLine(exception.GetType().FullName);
                stackTrace.AppendLine(exception.StackTrace);
            }

            return stackTrace.ToString();
        }
    }
}
