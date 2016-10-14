Interspecific
=============

Interspecific is a .NET 4.5 embedded HTTP/REST server with a focus on simplicity and a secondary focus
on performance.

Socket processing is based on  [MediaBrowser/SocketHttpListener](https://github.com/MediaBrowser/SocketHttpListener).
This is similar to HttpListener-based implementations, but it has several advantages:

- enables custom authentication headers (token based authentication)
- allows client software to listen on any port without admin priveleges
- does not require fiddling with the .NET framework's HTTP configuration. (This comes at the cost of .NET's built-in HTTPS support, but whether that is a disadvantage is debatable.)

## History ##

Interspecific started as a fork of Scott Offen's [Grapevine](https://github.com/scottoffen/Grapevine). It attempts to 
retain the Grapevine goal of simplicity, while making adding some complexity to the core engine to make
the actual REST server code even simpler, and improve efficiency and manage worst case failure modes
under heavy load conditions in production deployments.

## Features ##

- Embed a REST server in your application. Add attributes to your classes and methods to define resources and routes for managing traffic based on HTTP method and path info (using regular expressions).
- URL parsing based on regular expressions.
- Serve static files (HTML, CSS, JavaScript, images, etc.).
- Logging. Logging is pluggable, but is also a bit of a work-in-progress.

## Available on NuGet

https://www.nuget.org/packages/Interspecific/

## Contact ##

Submit bug reports and enhancements to Interspecific's Github Issues section.

## License ##
This project is licensed under the Apache License, V2.0.
Please see the LICENSE file at the root of this repository for details.
