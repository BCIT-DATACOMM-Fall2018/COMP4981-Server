using System;
using NetworkLibrary;
using NetworkLibrary.CWrapper;
using NetworkLibrary.MessageElements;
using System.Collections.Generic;

namespace Server
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			// Create a list of elements to send. Using the same list for unreliable and reliable
			List<UpdateElement> elements = new List<UpdateElement>();
			elements.Add(new HealthElement(15,6));

			// Create a UDPSocket
			UDPSocket socket = new UDPSocket ();
			// Bind the socket. Address must be in network byte order
			socket.Bind ((ushort)System.Net.IPAddress.HostToNetworkOrder ((short)8000));

			// Create a ReliableUDPConnection
			ReliableUDPConnection connection = new ReliableUDPConnection ();

			// Create a ServerStateMessageBridge to use later
			ServerStateMessageBridge bridge = new ServerStateMessageBridge ();

			while (true) {
				// Receive a packet. Receive calls block
				Packet packet = socket.Receive ();

				Console.WriteLine ("Got packet.");

				// Unpack the packet using the ReliableUDPConnection
				UnpackedPacket unpacked = connection.ProcessPacket (packet, new ElementId[] {ElementId.HealthElement});
				// Iterate through the unreliable elements and call their UpdateState function.
				foreach (var element in unpacked.UnreliableElements) {
					element.UpdateState (bridge);
				}

				Console.WriteLine ("Sending response packet.");
				// Create a new packet
				packet = connection.CreatePacket (elements, elements);

				// Send the packet
				socket.Send (packet, socket.LastReceivedFrom);
			}
		}
	}
}
