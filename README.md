# LiteNetLib-MemoryPack-Sample
Client and Server example utilizing LiteNetLib for networking and MemoryPack for high-performance serialization.


# Requirements
- MemoryPack: https://github.com/Cysharp/MemoryPack (Use NuGet or Source)
- LiteNetLib: https://github.com/RevenantX/LiteNetLib (Use Source)

# Solution Components
- Client
	- Provides an example implementation of the client-side networking code. It establishes a connection to the server and sends a "Hello" message upon successful connection. This component showcases how the client communicates with the server.

- Server
	- Contains the server-side logic required to handle incoming client connections. It listens for incoming "Hello" messages from the client and responds with a "Hello" message back to the client, demonstrating basic server-client communication.

- Shared
	- Includes shared networking utilities and components that are used across both the Client and Server. This project contains reusable code such as network message handling, logging utilities, and a singleton pattern implementation for centralized control.

- LiteNetLib
	- Contains the LiteNetLib library, a reliable UDP networking solution. The source code is referenced from LiteNetLib GitHub (https://github.com/RevenantX/LiteNetLib) repository and is used to handle low-level networking tasks for both the Client and Server.

