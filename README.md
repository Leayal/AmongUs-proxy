# AmongUs-Proxy

A simple solution to play LAN over internet without virtual-LAN and VPN.
This should help with those who want to play without going through the game's master server.
It's just like the host turning into a dedicated server.

## Note

- **This is NOT a game plugin or patch that modifies the game client by any means. This also means that this tool can be used for any game versions of the game as long as the game's netcode isn't changed drastically.**
- **The proxy needs to be kept running while you're playing. If you turn off the proxy tool while you're playing, the proxy tunnel will break. Thus, disconnecting you from the proxied game.**
- The GUI isn't robust.
- The proxy is that relays game packets around the game host and and the clients, it does **NOT** host any games, so you still need to host the game with your Among Us game client.
- While the proxy's core code is written in .NET Standard 2.0, I have no plans to write the application for mobile platform. However, please use it however you like if you have such plan.
- Normally, you **do NOT need to** run the proxy tool as Admin.

# If you are the game host:

- The server application is hardcoded to use the same port number for TCP and UDP (TCP for listening for incoming clients and UDP for game server relaying). Which means the host may need to setup the port-forwarding stuffs for both UDP and TCP for the proxy to work, clients don't need to do this.

1. Run the `Among Us` game and host a session/room as usually as if you're playing a normal LAN game.
2. Run the proxy tool, select `I want to host the game`.
3. \[Optional, can skip\] If you're advanced user, set the network interface and port as you desire. Otherwise, leave the network values alone.
4. Set the room name so that your friend can recognize the room's name from the list.
5. Click `start the proxy server` button. If Windows firewall popups, allow the proxy to bind to the network interface.
6. (Advanced step) Ensure that you've open NAT (a.k.a. Port-forwarding) correctly so that your friend could access to the proxy via Internet.

# If you are a client who connects to a host:

## - If the host is on the same local area network (same LAN):

- You don't need to use this tool, as the game supports same LAN officially.

## - If the host is on the different local area network (same LAN):

- The clients **DOES NOT** need to open NAT (or forward ports).

1. Run the proxy tool, select `I want to connect to someone's host`.
2. Enter the host's address. It could be a hostname or an IP address, and it could be with or without port. E.g: `amongus.example.com`, `amongus.example.com:3080`, `27.22.104.11` or `27.22.104.11:3070`. In case the port is omitted from the address, it will be assumed to be `3070` (`amongus.example.com` = `amongus.example.com:3070`)
3. \[Optional, but should be read\]:
   - In case you want to share the proxy throughout your whole local network so that your family members could play together with just one proxy client:
     - Uncheck the `Isolate the proxy to this device only`.
   - In case you are the only one who uses the proxy to play with the host:
     - Keep the `Isolate the proxy to this device only` checked. It is still okay if you uncheck this option as it works regardless.
4. Press `connect to the proxy server`. There may be a firewall popup, allow the proxy in firewall if it popups.
5. Run `Among Us` game and try to join the game as if it's an usual LAN game. The proxied game should appear in the LAN game list (The name of proxied game should usually start with `[Proxy]`).

# Credit

Special thank to [NickCis's among-us-proxy](https://github.com/NickCis/among-us-proxy) for the proof of concept.
