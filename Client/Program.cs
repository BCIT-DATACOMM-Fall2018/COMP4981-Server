using System;
using NetworkLibrary;
using NetworkLibrary.MessageElements;
using System.Collections.Generic;

namespace Client
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			// Create a list of elements to send. Using the same list for unreliable and reliable
			List<UpdateElement> elements = new List<UpdateElement>();
			elements.Add(new HealthElement(10,0));

			// Create a ClientStateMessageBridge to use later
			ClientStateMessageBridge bridge = new ClientStateMessageBridge ();

			// Create a UDPSocket
			UDPSocket socket = new UDPSocket ();
			// Bind the socket. No parameter means ephemeral port.
			socket.Bind ();

			// Create a ReliableUDPConnection
			ReliableUDPConnection connection = new ReliableUDPConnection ();

			// Create a packet using the connection
			Packet packet = connection.CreatePacket(elements, elements);

			// Create a destination to send to. Address and port must be in network order
			Destination destination = new Destination ((uint)System.Net.IPAddress.HostToNetworkOrder(2130706433), (ushort)System.Net.IPAddress.HostToNetworkOrder ((short)8000));

			Console.WriteLine ("Sending packet");
			// Send the packet to the destination
			socket.Send (packet, destination);

			// Receive a response packet. Receive calls block.
			packet = socket.Receive ();

			Console.WriteLine ("Got packet response");
			// Process the received packet
			UnpackedPacket unpacked = connection.ProcessPacket (packet, new ElementId[] {ElementId.HealthElement});
			// Iterate through the unreliable elements and call their UpdateState function.
			foreach (var element in unpacked.UnreliableElements) {
				element.UpdateState (bridge);
			}
			// Close the socket when done
			socket.Close ();
		}
	}
}
