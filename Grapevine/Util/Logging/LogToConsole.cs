using System;
using Interspecific.Server;

namespace Interspecific.Util.Logging
{
    public class LogToConsole : Interspecific.Util.Logging.ILog
    {
        public LogToConsole ()
        {
        }

        public void Log(string l)
        {
            System.Console.WriteLine (l);
        }

        public void Log(Exception ex)
        {
            System.Console.WriteLine (ex);
        }
    }
}

