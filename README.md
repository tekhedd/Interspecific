Moved
=====

This repository is now at https://gitlab.com/derbyinsight/Interspecific

Interspecific
=============

Interspecific is a .NET 4.5 embedded HTTP/REST server with a focus on simplicity and a secondary focus
on performance.

Sockets are handled by [a fork of](https://github.com/tekhedd/SocketHttpListener) MediaBrowser/SocketHttpListener.
The API is the same as standard HttpListener-based implementations, but it has several advantages:

- enables custom authentication headers (token based authentication)
- allows client software to listen on any port without admin priveleges
- does not require fiddling with the .NET framework's HTTP configuration. (This comes at the cost of .NET's built-in HTTPS support, but whether that is a disadvantage is debatable.)

## Features ##

- Embed a REST server in your application. 
- REST path and URL parsing based on regular expressions, compiled for efficiency.
- Serve static files.
- Logging via the standard TraceSource mechanism.

## Available on NuGet ##

https://www.nuget.org/packages/Interspecific/

## Versions ##

The 2.x versions may be considered stable, and will not incorporate any breaking
changes. Stable branch maintenance is currently in the master branch. 

## History ##

Interspecific started as a fork of Scott Offen's [Grapevine](https://github.com/scottoffen/Grapevine). It attempts to 
retain the Grapevine goal of simplicity, while making adding some complexity to the core engine to make
client code even simpler, improve efficiency, and manage worst case failure modes under heavy load conditions in production deployments.

## Contact ##

Submit bug reports and enhancements to Interspecific's Github Issues section.

## License ##

This project is licensed under the Apache License, V2.0.
Please see the LICENSE file at the root of this repository for details.
