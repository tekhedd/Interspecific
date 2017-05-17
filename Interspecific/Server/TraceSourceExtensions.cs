using System;
using System.Diagnostics;

namespace Interspecific.Server
{
    /// <summary>
    /// Extensions to the TraceSource class to make it more like Debug.Trace.
    internal static class TraceSourceExtensions
    {
        /// <summary>
        /// Add "TraceWarning" to the default TraceSource implementation.
        /// </summary>
        internal static void TraceWarning(this TraceSource ts, string format, params object[] args)
        {
            ts.TraceEvent(TraceEventType.Warning, 0, format, args);
        }
        
        /// <summary>
        /// Add "TraceError" to the default TraceSource implementation.
        /// </summary>
        internal static void TraceError(this TraceSource ts, string format, params object[] args)
        {
            ts.TraceEvent(TraceEventType.Error, 0, format, args);
        }
    }
}

