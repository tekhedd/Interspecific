using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

using SocketHttpListener.Net;

namespace Interspecific.Server
{
    /// <summary>
    /// Delegate for pre and post starting and stopping of RESTServer
    /// </summary>
    public delegate void ToggleServerHandler();

    public class RESTServer : Responder, IDisposable
    {
        #region Instance Variables

        private readonly RouteCache _routeCache;

        private readonly Thread[] _workers;

        private readonly HttpListener _listener;
        private readonly ManualResetEvent _stop = new ManualResetEvent(false);
        private readonly ManualResetEvent _ready = new ManualResetEvent(false);
        private Queue<HttpListenerContext> _queue = new Queue<HttpListenerContext>();
        
        /// <summary>
        /// Trace source for the Interspecific.Server namespace. Add trace listeners
        /// </summary>
        private readonly FunctionalTraceSource _trace;
        
        /// <summary>
        /// Trace source for the HttpListener
        /// </summary>
        private readonly TraceSource _httpTraceSource;
        
        #endregion

        #region Constructors

        [Obsolete( "Please use RESTServer(config,tag)" )]
        public RESTServer(string host = "localhost", string port = "1234", string protocol = "http", string dirindex = "index.html", string webroot = null, int maxthreads = 5, object tag = null)
         : this( 
            new Config { 
                Host = host,
                Port = port,
                Protocol = protocol,
                DirIndex = dirindex, 
                WebRoot = webroot, 
                MaxThreads = maxthreads }, 
            tag )
        {
        }

        public RESTServer(Config config, object tag = null) 
        {
            this._trace = new FunctionalTraceSource( config.TraceSourceName, config.TraceSourceLevel );
            
            this._httpTraceSource = new TraceSource(config.TraceSourceName + ".HttpListener", config.TraceSourceLevel);
            
            // TODO: update HttpListener to use Trace / TraceSource.
            this._listener = new HttpListener(new GenericTraceLogger(this._httpTraceSource));
            
            // TODO: don't use hard coded timeouts
            _listener.TimeoutManager.DrainEntityBody = TimeSpan.FromSeconds( 60 );
            _listener.TimeoutManager.EntityBody = TimeSpan.FromSeconds( 30 );
            _listener.TimeoutManager.HeaderWait = TimeSpan.FromSeconds( 30 );
            _listener.TimeoutManager.IdleConnection = TimeSpan.FromSeconds( 120 );
            
            this.IsListening = false;
            this.DirIndex = config.DirIndex;
      
            this.Host = config.Host;
            this.Port = config.Port;
            this.Protocol = config.Protocol;
            this.MaxThreads = config.MaxThreads;
            this.AutoLoadRestResources = config.AutoLoadRestResources;
            this.Tag = tag;
            this.ServerHeader = config.ServerHeader;
      
            if (config.WebRoot == null)
            {
                this.WebRoot = Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location), "webroot");
            }
            else
            {
                this.WebRoot = config.WebRoot;
            }
         
            this._routeCache = new RouteCache(this, this.BaseUrl, this.AutoLoadRestResources);
            this._workers = new Thread[this.MaxThreads];
            this._listener.OnContext += new Action<HttpListenerContext>(QueueRequest);
        }
        
        #endregion
        #region Public Properties

        /// <summary>
        /// Delegate used to execute custom code before attempting server start
        /// </summary>
        public ToggleServerHandler OnBeforeStart { get; set; }

        /// <summary>
        /// Delegate used to execute custom code after successful server start
        /// </summary>
        public ToggleServerHandler OnAfterStart { get; set; }

        /// <summary>
        /// Delegate used to execute custom code before attempting server stop
        /// </summary>
        public ToggleServerHandler OnBeforeStop { get; set; }

        /// <summary>
        /// Delegate used to execute custom code after successful server stop
        /// </summary>
        public ToggleServerHandler OnAfterStop { get; set; }

        /// <summary>
        /// Delegate used to execute custom code after successful server start; synonym for OnAfterStart
        /// </summary>
        public ToggleServerHandler OnStart
        {
            get
            {
                return this.OnAfterStart;
            }
            set
            {
                this.OnAfterStart = value;
            }
        }

        /// <summary>
        /// Delegate used to execute custom code after successful server stop; synonym for OnAfterStop
        /// </summary>
        public ToggleServerHandler OnStop
        {
            get
            {
                return this.OnAfterStop;
            }
            set
            {
                this.OnAfterStop = value;
            }
        }

        /// <summary>
        /// Returns true if the server is currently listening for incoming traffic
        /// </summary>
        public bool IsListening { get; private set; }

        /// <summary>
        /// Default file to return when a directory is requested without a file name
        /// </summary>
        public string DirIndex { get; set; }

        /// <summary>
        /// Specifies the top-level directory containing website content; 
        /// directory will be created if it does not exist. On failure, exceptions
        /// will be thrown.
        /// </summary>
        public string WebRoot
        {
            get
            {
                return this._webroot;
            }
            set
            {
                if (null == value)
                    throw new ArgumentException("may not be null", "webroot");
                    
                if (!Directory.Exists(value))
                    Directory.CreateDirectory(value);
                    
                this._webroot = value;
            }
        }
        private string _webroot;

        /// <summary>
        /// <para>The host name used to create the HttpListener prefix</para>
        /// <para>&#160;</para>
        /// <para>Use "*" to indicate that the HttpListener accepts requests sent to the port if the requested URI does not match any other prefix. Similarly, to specify that the HttpListener accepts all requests sent to a port, replace the host element with the "+" character.</para>
        /// </summary>
        public string Host
        {
            get
            {
                return this._host;
            }
            set
            {
                if (this.IsListening)
                    throw new ServerStateException("Attempted to modify Host property after server start.");

                this._host = value;
            }
        }
        private string _host;

        /// <summary>
        /// The port number (as a string) used to create the HttpListener prefix
        /// </summary>
        public string Port
        {
            get
            {
                return this._port;
            }
            set
            {
                if (this.IsListening)
                    throw new ServerStateException("Attempted to modify Port property after server start.");
                    
                this._port = value;
            }
        }
        private string _port;

        /// <summary>
        /// Returns the prefix created by combining the Protocol, Host and Port properties
        /// </summary>
        public string BaseUrl
        {
            get
            {
                return String.Format("{0}://{1}:{2}/", this.Protocol, this.Host, this.Port);
            }
        }

        /// <summary>
        /// The number of threads that will be started to respond to queued requests
        /// </summary>
        public int MaxThreads
        {
            get
            {
                return this._maxt;
            }
            set
            {
                if (this.IsListening)
                    throw new ServerStateException("Attempted to modify MaxThreads property after server start.");
                    
                if (value < 1)
                    throw new ArgumentException("cannot be less than 1", "MaxThreads");

                this._maxt = value;
            }
        }
        private int _maxt;
  
        /// <summary>
        /// Should the app automatically search for RESTRoute-decorated objects in the application path?
        /// </summary>
        public bool AutoLoadRestResources { set; get; }

        /// <summary>
        /// Maximum number of outstanding requests that will be queued for
        /// processing.
        /// </summary>
        public int MaxPendingRequests
        {
            set
            {
                if (value < 1)
                    throw new ArgumentException("cannot be less than 1", "MaxPendingRequests");
                
                _maxRequests = value;
            }
            get
            {
                return _maxRequests;
            }
        }
        private int _maxRequests = 200;

        /// <summary>
        /// <para>The URI scheme (or protocol) to be used in creating the HttpListener prefix; ex. "http" or "https"</para>
        /// <para>&#160;</para>
        /// <para>Note that if you create an HttpListener using https, you must select a Server Certificate for that listener. See the MSDN documentation on the HttpListener class for more information.</para>
        /// </summary>
        public string Protocol
        {
            get
            {
                return _protocol;
            }
            set
            {
                if (this.IsListening)
                    throw new ServerStateException("Attempted to modify Protocol property after server start.");
                
                this._protocol = value;
            }
        }
        private string _protocol;

        /// <summary>
        /// Arbitary object to tag the server with.
        /// </summary>
        /// <value>The tag.</value>
        public object Tag 
        {
            get;
            set;
	}
  
        /// <summary>
        /// Content of the HTTP header "Server:". 
        /// </summary>
        public string ServerHeader
        {
            get;
            set;
        }

        #endregion

        #region Public Methods
  
        /// <summary>
        /// Manually register a rest resource. The resource will be adopted by this class.
        /// </summary>
        /// <example>
        ///   server = new RESTServer( Config { AutoLoadRestResource = false; } );
        ///   server.AddResource( new MyRestResource() );
        ///   server.Start();
        /// </example>
        public void AddResource( RESTResource resource )
        {
            _routeCache.Add( resource );
        }

        /// <summary>
        /// Attempts to start the server; executes delegates for OnBeforeStart and OnAfterStart
        ///
        /// Throws exceptions if the server fails to start.
        /// </summary>
        /// 
        ///
        public void Start()
        {
            if (!this.IsListening)
            {
                try
                {
                    this.FireDelegate(this.OnBeforeStart);

                    this._routeCache.SortRoutes();
                    this.IsListening = true;
                    this._listener.Prefixes.Add(this.BaseUrl);

                    this._stop.Reset();
                    this._listener.Start();

                    for (int i = 0; i < _workers.Length; i++)
                    {
                        _workers[i] = new Thread(Worker);
                        _workers[i].Name = String.Format("HTTP Listener[{0}]", i);
                        _workers[i].Start();
                    }

                    this.FireDelegate(this.OnAfterStart);
                }
                catch (Exception)
                {
                    try 
                    {
                        this._stop.Set();
                        // TODO: clean up any created worker threads
                        this._listener.Stop();
                    }
                    catch (Exception nested) 
                    {
                        // do nothing
                        _trace.TraceInformation("Nested exception in listener startup exception handling: {0}", 
                                                nested.ToString());
                    }
                    
                    this.IsListening = false;
                    throw;
                }
            }
        }

        public void Stop()
        {
            Stop(TimeSpan.FromMinutes(5));
        }

        /// <summary>
        /// Attempts to stop the server; executes delegates for OnBeforeStop and OnAfterStop
        /// 
        /// After timeout is reached, threads and remote connections will be killed 
        /// without waiting for data.
        /// </summary>
        public void Stop(TimeSpan timeout)
        {
            DateTime startTime = DateTime.Now;
            
            this.FireDelegate(this.OnBeforeStop);

            this._stop.Set();      // Signal worker threads to gracefully stop

            // Use the first half of the timeout for graceful closure.
            
            TimeSpan halfTime = new TimeSpan(timeout.Ticks / 2);
            foreach (Thread worker in this._workers)
            {
                TimeSpan timeLeft = halfTime - (DateTime.Now - startTime);
                if (timeLeft < TimeSpan.Zero)
                    timeLeft = TimeSpan.Zero;

                worker.Join(timeLeft);
            }
           
            this._listener.Stop(); // Signal listener to ungracefully stop/forcefully close connections
           
            // If any workers still won't exit, forcefully abort them.
            foreach (Thread worker in this._workers)
            {
                if (worker.IsAlive) 
                {
                    TimeSpan timeLeft = timeout - (DateTime.Now - startTime);
                    if (timeLeft < TimeSpan.Zero)
                        timeLeft = TimeSpan.Zero;
                        
                    _trace.TraceInformation("Waiting for unresponsive worker thread: {0} ({1})", worker.Name, timeLeft);
                    worker.Join(timeLeft);
                    
                    if (worker.IsAlive) // WHY WON'T YOU DIE?
                    {
                        _trace.TraceWarning("Aborting unresponsive HTTP worker thread: {0}", worker.Name);
                        worker.Abort();
                    }
                }
            }
           
            this.IsListening = false;

            this.FireDelegate(this.OnAfterStop);
        }

        /// <summary>
        /// Implementation of the IDisposable interface; explicitly calls the Stop() method
        /// </summary>
        public void Dispose()
        {
            this.Stop();
        }

        /// <summary>
        /// Add trace listeners to this server's trace source(s)
        /// </summary>
        public void AddTraceListener(TraceListener listener)
        {
            _trace.Listeners.Add(listener);
            _httpTraceSource.Listeners.Add(listener);
        }
        
        /// <summary>
        /// Remove all trace listeners.
        /// </summary>
        public void ClearTraceListeners()
        {
            _trace.Listeners.Clear();
            _httpTraceSource.Listeners.Clear();
        }

        #endregion

        #region Private Threading Methods

        private void QueueRequest(HttpListenerContext context)
        {
            lock (this._queue)
            {
                if (_queue.Count > MaxPendingRequests)
                {
                    context.Response.StatusCode = 503;
                    context.Response.OutputStream.Close();
                    context.Response.Close();
                    
                    _trace.TraceWarning("Request queue max size reached: {0}. Connection refused with 503 error.", 
                                        MaxPendingRequests);
                    return;
                }

                this._queue.Enqueue(context);
                this._ready.Set();
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            try
            {
                if (ServerHeader != null)
                    context.Response.Headers["Server"] = ServerHeader;
            
                var notfound = true;
                if (this._routeCache.FindAndInvokeRoute(context))
                {
                    notfound = false;
                }
                else if ((context.Request.HttpMethod.ToUpper().Equals("GET")) && (!object.ReferenceEquals(this.WebRoot, null)))
                {
                    var filename = this.GetFilePath( context.Request.Url.LocalPath );
                    if (!object.ReferenceEquals(filename, null))
                    {
                        this.SendFileResponse(context, filename);
                        notfound = false;
                    }
                }

                if (notfound)
                {
                    this.NotFound(context);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    _trace.TraceInformation("Internal error in ProcessRequest(): {0}", ex);
                    this.InternalServerError(context, ex);
                }
                catch (Exception nested) // We can't even serve an error?
                {
                    _trace.TraceWarning("Nested internal error: could not serve Error 500: {0}", nested.ToString());
                    context.Response.StatusCode = 500; // Maybe we can serve the code
                }
            }
            finally
            {
                context.Response.OutputStream.Close(); // prevent resource leaks
                context.Response.Close(); // paranoia
            }
        }

        private void Worker()
        {
            WaitHandle[] wait = new[] { this._ready, this._stop };
            while (0 == WaitHandle.WaitAny(wait))
            {
                try
                {
                    HttpListenerContext context;
                    lock (this._queue)
                    {
                        if (this._queue.Count > 0)
                        {
                            context = this._queue.Dequeue();
                        }
                        else
                        {
                            this._ready.Reset();
                            continue;
                        }
                    }

                    this.ProcessRequest(context);
                }
                catch (Exception ex)
                {
                    try
                    {
                        _trace.TraceError("Unhandled exception in RESTServer worker thread: {0}", ex);
                    }
                    catch
                    {
                        // Don't let the worker thread die
                    }
                }
            }
        }

        #endregion

        #region Private Utility Methods

        private string GetFilePath(string urlPath)
        {
            string filename = urlPath.TrimStart('/', '\\');
            var path = Path.Combine(this.WebRoot, filename);

            if (File.Exists(path))
            {
                return path;
            }
            else if (Directory.Exists(path))
            {
                path = Path.Combine(path, this.DirIndex);
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        /// <summary>
        /// Fire one of the pre/post/other delegates.
        /// Delegate exceptions propagate, to be handled by the 
        /// calling code.
        /// </summary>
        private void FireDelegate(ToggleServerHandler method)
        {
            if (!object.ReferenceEquals(method, null))
            {
                method();
            }
        }

        #endregion
    }
}