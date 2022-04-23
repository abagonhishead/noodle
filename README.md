# Noodle
Noodle is a small REST API that sends wake-on-LAN 'magic' UDP broadcast packets on a local network. It is written in ASP.NET Core, .NET 6 & .NET Standard 2.1.

## Using it
You can trigger a magic packet by sending an HTTP POST request to /magicpacket/send with the following payload:
````json
{
  "targetMacAddress": "de:ad:b3:3f:7a:c0",
  "broadcastAddress": "10.0.0.255"
}
````
Replacing `targetMacAddress` with the hardware MAC address of the target machine, and `broadcastAddress` with the IPv4 broadcast address of the subnet the machine sits on.

IPv6 is not currently supported.

The application includes Swagger/OpenAPI specs. -- documentation is in the usual place at `/swagger/index.html`. 

## Building/deploying it
You should be able to build and deploy this in the normal way you build & deploy any ASP.NET Core web app. I'm not currently providing releases/builds, so you'll have to clone the repo and build it yourself for now.

## Running it
### Linux
I recommend running it behind nginx with either systemd socket activation:

`dotnet run Jossellware.Noodle.Web.Api.dll --unixLifetime:useSystemdSocket=true`

Or a standard UNIX socket, if your distro of choice doesn't use systemd:

`dotnet run Jossellware.Noodle.Web.Api.dll --unixLifetime:useSystemdSocket=false --unixLifetime:managedSocketPath=/run/noodle.sock`

It uses the standard .NET 6 configuration classes, so the above switches can also be set in `appsettings.json`. 

Obviously you'll need to point your nginx site at the socket path.

### Docker
It will also run fine on a Linux docker container using the provided Dockerfile, but it requires a fair bit of configuration to get the UDP broadcast packets onto your LAN. 

The most success I had with running this in a container was to use a macvlan interface (Linux-only, see: [this blog post](https://blog.oddbit.com/post/2018-03-12-using-docker-macvlan-networks/) for pointers). This means the container appears as if it's physically connected to the local network of a host interface, but this is a clunky solution -- docker still has to assign the containers their IP addresses (rather than the LAN's usual DHCP server), and also has to be informed of the physical network configuration. The macvlan interface also needs to be configured separately using `systemd-networkd` (or your equivalent network configurator). All of this makes deployment a bit more awkward.

If anyone finds a better way of getting UDP broadcast packets routed across from the docker virtual interface to a hardware ethernet interface then please let me know. In the meantime, I recommend running this on a physical machine.

### Everything else
This runs fine on Windows, but I've only tested it while debugging.

I haven't tested this on BSD or OS X at all, so YMMV. My BSD knowledge is limited, but BSD's routing tools might result in more success getting the packets routed from a container onto a LAN!

## Development
It is still a work-in-progress. It needs:

- Unit tests
- A tidy up of the bootstrap class 
- Migration of the static bootstrap class to an `IStartup` implementation or something similar
- Static helpers replacing with instanced implementations (`InteropHelper`, `UnixHelper`) because nobody likes static helpers
- Some digging to figure out if `.ListenUnixSocket()` still has that permissions bug, as the socket permissions `Task` is an awful hacky workaround. It should also probably be updated to octal 0660 instead of 0770.

In future, I plan on adding:
- Builds/releases
- An accompanying command-line application for sending the packets directly
- IPv6 support
- Possibly a gRPC API

I will consider contributions but please get in touch first. Don't raise a PR out of the blue!

## Licence
This software is provided under the MIT licence (see below). You're free to clone, fork and re-use this software as you see fit. Obviously you don't have to attribute to me, but if you fork and make general improvements please consider letting me merge them in.

---
Copyright © 2022 Russell Webster, Jossellware

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
