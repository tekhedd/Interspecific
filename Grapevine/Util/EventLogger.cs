using System;
using System.Diagnostics;
using System.Text;
using Interspecific.Util.Logging;

namespace Interspecific
{
    /// <summary>
    /// Static class for recording logs in an EventLog. The EventLog property must be set or nothing will be logged.
    /// </summary>
    public static class EventLogger
    {

        private static ILog _logImplementation;
        public static ILog LogImplementation 
        {
            get 
            {
                if (_logImplementation == null)
                    _logImplementation = new LogToEventLog ();
                return _logImplementation;
            }
            set 
            {
                _logImplementation = value;
            }
        }

        /// <summary>
        /// If LogExceptions is true, writes the result of ExceptionToString(e) to the event log
        /// </summary>
        public static void Log(Exception e)
        {
            LogImplementation.Log (e);
        }

        /// <summary>
        /// Logs the message to the EventLog
        /// </summary>
        public static void Log(String message)
        {
            LogImplementation.Log (message);
        }
    }
}
