using System;
using System.Diagnostics;
using System.Text;

using Patterns.Logging;

namespace Interspecific.Server
{
    /// <summary>
    /// Implements the emby Patterns.Logging.ILogger as a generic Trace
    /// logger.
    ///
    /// Note: tracing expects our TraceSourceExtensions implementation.
    /// </summary>
    internal class GenericTraceLogger : ILogger
    {
        private readonly TraceSource _source;
        
        public GenericTraceLogger(TraceSource source)
        {
            _source = source;
        }
        
        public void Info(string message, params object[] paramList)
        {
            _source.TraceEvent(TraceEventType.Information, 0, message, paramList);
        }

        public void Error(string message, params object[] paramList)
        {
            _source.TraceEvent(TraceEventType.Error, 0, message, paramList);
        }

        public void Warn(string message, params object[] paramList)
        {
            _source.TraceEvent(TraceEventType.Warning, 0, message, paramList);
        }

        public void Debug(string message, params object[] paramList)
        {
            _source.TraceEvent(TraceEventType.Verbose, 0, message, paramList);
        }

        public void Fatal(string message, params object[] paramList)
        {
            _source.TraceEvent(TraceEventType.Error, 0, message, paramList);
        }

        public void FatalException(string message, Exception exception, params object[] paramList)
        {
            _source.TraceEvent(TraceEventType.Critical, 0, message, paramList);
        }

        public void Log(LogSeverity severity, string message, params object[] paramList)
        {
            TraceEventType tType;
            switch (severity)
            {
            case LogSeverity.Debug:   tType = TraceEventType.Verbose;     break;
            case LogSeverity.Info:    tType = TraceEventType.Information; break;
            case LogSeverity.Warn:    tType = TraceEventType.Warning;     break;
            case LogSeverity.Error:   tType = TraceEventType.Error;       break;
            case LogSeverity.Fatal:   tType = TraceEventType.Critical;    break;
            
            default:
                throw new ApplicationException("Unexpected case in switch");
            }
            
            _source.TraceEvent(tType, 0, message, paramList);
        }

        public void ErrorException(string message, Exception exception, params object[] paramList)
        {
            this.Error(message, exception, paramList);
        }

        public void LogMultiline(string message, LogSeverity severity, StringBuilder additionalContent)
        {
            this.Log(severity, message);
            this.Log(severity, additionalContent.ToString()); // a bit of a hack but it'll do for now.
        }
    }
}

