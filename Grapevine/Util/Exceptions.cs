using System;

namespace Grapevine
{
    /// <summary>
    /// The exception that is thrown when there is an attempt to modify the Protocol, Host, Port or MaxThreads property of a running instance of RESTServer.
    /// </summary>
    public class ServerStateException : Exception
    {
        public ServerStateException() : base("Protocol, Host, Port and MaxThreads cannot be modified while the server is running.") { }

        public ServerStateException(string message) : base(message) { }

        public ServerStateException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// The exception that is thrown when attempting to access the PathInfo property of a RESTRequest where not all resource tokens are able to be resolved.
    /// </summary>
    public class ClientStateException : Exception
    {
        public ClientStateException() : base("Unresolved Resource Tokens") { }

        public ClientStateException(string message) : base(message) { }

        public ClientStateException(string message, Exception inner) : base(message, inner) { }
    }
   
   /// <summary>
   /// Same as ArgumentException, but subclassed so you can detect that it is specifically
   /// a URL or query parameter that is invalid (and map to a 400 error if desired).
   /// </summary>
   public class RESTArgumentException : ArgumentException
   {
      public RESTArgumentException(string message, string name) : base(message, name) { }
   }
}
