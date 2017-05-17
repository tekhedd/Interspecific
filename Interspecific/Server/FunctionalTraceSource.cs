using System;
using System.Diagnostics;

namespace Interspecific.Server
{
    /// <summary>
    /// Wrapper around TraceSource, because apparently TraceSource.TraceInformation() is
    /// a noop in mono 4.8.1. 
    /// </summary>
    internal class FunctionalTraceSource
    {
        private readonly TraceSource ts;
        
        internal FunctionalTraceSource(string name, SourceLevels level)
        {
            ts = new TraceSource( name, level );
        }
        
        internal TraceListenerCollection Listeners
        {
            get { return ts.Listeners; }
        }
        
        internal void TraceInformation(string format, params object[] args)
        {
            ts.TraceEvent(TraceEventType.Information, 0, format, args);
        }
        
        /// <summary>
        /// Add "TraceWarning" to the default TraceSource implementation.
        /// </summary>
        internal void TraceWarning(string format, params object[] args)
        {
            ts.TraceEvent(TraceEventType.Warning, 0, format, args);
        }
        
        /// <summary>
        /// Add "TraceError" to the default TraceSource implementation.
        /// </summary>
        internal void TraceError(string format, params object[] args)
        {
            ts.TraceEvent(TraceEventType.Error, 0, format, args);
        }
    }
}

