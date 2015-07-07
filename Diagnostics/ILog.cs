﻿using System;
using System.Collections.Generic;
using Octopus.Shared.Logging;
using Octopus.Shared.Properties;

namespace Octopus.Shared.Diagnostics
{
    public interface ILog
    {
        LogCorrelator Current { get; }
        bool IsVerboseEnabled { get; }
        bool IsErrorEnabled { get; }
        bool IsFatalEnabled { get; }
        bool IsInfoEnabled { get; }
        bool IsTraceEnabled { get; }
        bool IsWarnEnabled { get; }

        /// <summary>
        /// Opens a new child block for logging.
        /// </summary>
        /// <param name="messageText">Title of the new block.</param>
        /// <returns>An <see cref="IDisposable" /> that will automatically revert the current block when disposed.</returns>
        IDisposable OpenBlock(string messageText);

        /// <summary>
        /// Opens a new child block for logging.
        /// </summary>
        /// <param name="messageFormat">Format string for the message.</param>
        /// <param name="args">Arguments for the format string.</param>
        /// <returns>An <see cref="IDisposable" /> that will automatically revert the current block when disposed.</returns>
        [StringFormatMethod("messageFormat")]
        IDisposable OpenBlock(string messageFormat, params object[] args);

        /// <summary>
        /// Plans a new block of output that will be used in the future for grouping child blocks for logging.
        /// </summary>
        /// <param name="messageText">Title of the new block.</param>
        /// <returns>An <see cref="LogCorrelator" /> that will automatically revert the current block when disposed.</returns>
        LogCorrelator PlanGroupedBlock(string messageText);

        /// <summary>
        /// Plans a new block of log output that will be used in the future. This is typically used for high-level log
        /// information, such as the steps in a big deployment process.
        /// </summary>
        /// <param name="messageText">Title of the new block.</param>
        /// <returns>An <see cref="IDisposable" /> that will automatically revert the current block when disposed.</returns>
        LogCorrelator PlanFutureBlock(string messageText);

        /// <summary>
        /// Plans a new block of log output that will be used in the future. This is typically used for high-level log
        /// information, such as the steps in a big deployment process.
        /// </summary>
        /// <param name="messageFormat">Format string for the message.</param>
        /// <param name="args">Arguments for the format string.</param>
        /// <returns>An <see cref="IDisposable" /> that will automatically revert the current block when disposed.</returns>
        [StringFormatMethod("messageFormat")]
        LogCorrelator PlanFutureBlock(string messageFormat, params object[] args);

        /// <summary>
        /// Switches to a new logging context on the current thread, allowing you to begin logging within a block previously
        /// begun using <see cref="OpenBlock" /> or <see cref="PlanFutureBlock" />.
        /// </summary>
        /// <param name="logger">The <see cref="LogCorrelator" /> to switch to.</param>
        /// <returns>An <see cref="IDisposable" /> that will automatically revert the current block when disposed.</returns>
        IDisposable WithinBlock(LogCorrelator logger);

        /// <summary>
        /// Marks the current block as abandoned. Abandoned blocks won't be shown in the task log.
        /// </summary>
        void Abandon();

        /// <summary>
        /// If a block was previously abandoned using <see cref="Abandon" />, calling <see cref="Reinstate" /> will un-abandon
        /// it.
        /// </summary>
        void Reinstate();

        /// <summary>
        /// Marks the current block as finished. If there were any errors, it will be finished with errors. If there were no
        /// errors, it will be assumed to be successful.
        /// </summary>
        void Finish();

        /// <summary>
        /// Imports a set of log events into the current context.
        /// </summary>
        /// <param name="logEvents">The set of log events.</param>
        void Merge(IEnumerable<LogEvent> logEvents);

        bool IsEnabled(LogCategory category);
        void Write(LogCategory category, string messageText);
        void Write(LogCategory category, Exception error);
        void Write(LogCategory category, Exception error, string messageText);

        [StringFormatMethod("messageFormat")]
        void WriteFormat(LogCategory category, string messageFormat, params object[] args);

        [StringFormatMethod("messageFormat")]
        void WriteFormat(LogCategory category, Exception error, string messageFormat, params object[] args);

        void Trace(string messageText);
        void Trace(Exception error);
        void Trace(Exception error, string messageText);

        [StringFormatMethod("messageFormat")]
        void TraceFormat(string messageFormat, params object[] args);

        [StringFormatMethod("format")]
        void TraceFormat(Exception error, string format, params object[] args);

        void Verbose(string messageText);
        void Verbose(Exception error);
        void Verbose(Exception error, string messageText);

        [StringFormatMethod("messageFormat")]
        void VerboseFormat(string messageFormat, params object[] args);

        [StringFormatMethod("format")]
        void VerboseFormat(Exception error, string format, params object[] args);

        void Info(string messageText);
        void Info(Exception error);
        void Info(Exception error, string messageText);

        [StringFormatMethod("messageFormat")]
        void InfoFormat(string messageFormat, params object[] args);

        [StringFormatMethod("format")]
        void InfoFormat(Exception error, string format, params object[] args);

        void Warn(string messageText);
        void Warn(Exception error);
        void Warn(Exception error, string messageText);

        [StringFormatMethod("messageFormat")]
        void WarnFormat(string messageFormat, params object[] args);

        [StringFormatMethod("format")]
        void WarnFormat(Exception error, string format, params object[] args);

        void Error(string messageText);
        void Error(Exception error);
        void Error(Exception error, string messageText);

        [StringFormatMethod("messageFormat")]
        void ErrorFormat(string messageFormat, params object[] args);

        [StringFormatMethod("format")]
        void ErrorFormat(Exception error, string format, params object[] args);

        void Fatal(string messageText);
        void Fatal(Exception error);
        void Fatal(Exception error, string messageText);

        [StringFormatMethod("messageFormat")]
        void FatalFormat(string messageFormat, params object[] args);

        [StringFormatMethod("format")]
        void FatalFormat(Exception error, string format, params object[] args);

        void UpdateProgress(int progressPercentage, string messageText);

        [StringFormatMethod("messageFormat")]
        void UpdateProgressFormat(int progressPercentage, string messageFormat, params object[] args);

        void Flush();
    }
}