Interspecific
=============

Interspecific is a .NET 4.5 embedded HTTP/REST server with a focus on simplicity and a secondary focus
on performance.

Socket processing is based on [(a fork of)](https://github.com/tekhedd/SocketHttpListener) [MediaBrowser/SocketHttpListener](https://github.com/MediaBrowser/SocketHttpListener).
This is similar to HttpListener-based implementations, but it has several advantages:

- enables custom authentication headers (token based authentication)
- allows client software to listen on any port without admin priveleges
- does not require fiddling with the .NET framework's HTTP configuration. (This comes at the cost of .NET's built-in HTTPS support, but whether that is a disadvantage is debatable.)

## Features ##

- Embed a REST server in your application. Add attributes to your classes and methods to define resources and routes for managing traffic based on HTTP method and path info (using regular expressions).
- URL parsing based on regular expressions.
- Serve static files (HTML, CSS, JavaScript, images, etc.).
- Logging. Logging is pluggable, but is also a bit of a work-in-progress.

## Versions ##

The currrent stable releases are in the "1.x" branch and are released with v1 versions. Stable releases do not remove deprecated features and only include bug fixes.

Unstable development is in branch "master", and these are currently released as version 2.x releases. The API may change and new features may be added. If you feel that you need unstable features backported to 1.x or would like to see 2.x feature frozen please open an issue. Or in fact just drop me a line if you're using the library.

## Available on NuGet ##

https://www.nuget.org/packages/Interspecific/

## History ##

Interspecific started as a fork of Scott Offen's [Grapevine](https://github.com/scottoffen/Grapevine). It attempts to 
retain the Grapevine goal of simplicity, while making adding some complexity to the core engine to make
the actual REST server code even simpler, and improve efficiency and manage worst case failure modes
under heavy load conditions in production deployments.

## Contact ##

Submit bug reports and enhancements to Interspecific's Github Issues section.

## License ##
This project is licensed under the Apache License, V2.0.
Please see the LICENSE file at the root of this repository for details.
