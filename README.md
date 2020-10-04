# AmongUs-Proxy
A simple solution to play LAN over internet without virtual-LAN and VPN.
This should help with those who want to play without going through the game's master server.
It's just like the host turning into a dedicated server.

## Notice
- The GUI isn't robust.
- The server application is hardcoded to use the same port number for TCP and UDP (TCP for listening for incoming clients and UDP for game server relaying). Which means the host may need to setup the port-forwarding stuffs, clients don't need to do this.
- The proxy is just a tunnel to relay packets, it does **not** host any games, so you still need to host the game with your Among Us game client.
- While the proxy's core code is written in .NET Standard 2.0, I have no plans to write the application for mobile platform. However, please use it however you like if you have such plan.

## Credit
Special thank to [NickCis's among-us-proxy](https://github.com/NickCis/among-us-proxy) for the proof of concept.