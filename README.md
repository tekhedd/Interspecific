Interspecific
=============

Interspecific is a .NET 4.0 embedded HTTP/REST server with a focus on simplicity and a secondary focus
on performance.

Socket processing is based on  [MediaBrowser/SocketHttpListener](https://github.com/MediaBrowser/SocketHttpListener).
This is different from HttpListener-based implementations, because it enables custom authentication headers, 
allows client software to listen on any port without admin priveleges, and does not require 
fiddling with the .NET framework's HTTP configuration. This comes at the cost of .NET's built-in
HTTPS support, but whether that is a disadvantage is debatable.

## History ##

Interspecific is a fork of Scott Offen's Grapevine. It attempts to 
retain the Grapevine goal of simplicity, while making adding some complexity to the core engine to make
the actual REST server code even simpler, and improve efficiency under heavy load conditions in production
deployments.

If Interspecific seems too complex, I highly recommend [Grapevine](https://github.com/scottoffen/Grapevine) or
its [Grapevine Plus](https://github.com/scottoffen/Grapevine) cousin.

## Features ##

- Embed a REST server in your application. Add attributes to your classes and methods to define resources and routes for managing traffic based on HTTP method and path info (using regular expressions). The [message context](http://msdn.microsoft.com/en-us/library/vstudio/system.net.httplistenercontext(v=vs.110).aspx) is passed to your route every time, and each resource has a reference to the server that spawned it.

- URL parsing based on regular expressions.

- Manage multiple REST servers simultaneously and easily with a `RESTCluster`. Scope your resources to one, many or all REST servers.

- Serve up static files (HTML, CSS, JavaScript, images, etc.) with virtually no configuration. Each server can have a unique location to serve files from, or they can all share a location.

- Logging. Logging is pluggable, but is also a bit of a work-in-progress.

## Limitations ##

- Interspecific is **not** intended as a drop-in replacement for [Microsoft IIS](http://www.iis.net/) or [Apache HTTP Server](http://httpd.apache.org/). Nor is it a full-featured application server (like [Tomcat](http://en.wikipedia.org/wiki/Apache_Tomcat)) or framework (like [Spring](http://en.wikipedia.org/wiki/Spring_Framework)). Interspecific is **embedded in your application**, where using one of those would be impossible, or just plain overkill.

- Interspecific does not do any script parsing (**PHP**, **Perl**, **Python**, **Ruby**, etc.). Presumably you will do 
in C# what would otherwise require a script.

- You will likely be required to [open a port in your firewall](http://www.lmgtfy.com/?q=how+to+open+a+port+on+windows) for remote computers to be able to send requests to your application. Grapevine will not [automatically](http://msdn.microsoft.com/en-us/library/aa366418%28VS.85%29.aspx) do that for you.  You might want to do that during the [installation of your application](http://www.codeproject.com/Articles/14906/Open-Windows-Firewall-During-Installation).

## Contact ##

Please don't bother Scott with Interspecific questions; we have diverged enough that this is not appropriate.
Submit bug reports and enhancements to Interspecific's Github Issues section.

## License ##
This project is licensed under the Apache License, V2.0.
Please see the LICENSE file at the root of this repository for details.
